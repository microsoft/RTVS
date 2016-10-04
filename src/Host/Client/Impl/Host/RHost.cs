// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public sealed partial class RHost : IDisposable, IRExpressionEvaluator, IRBlobService {
        private static readonly Task<REvaluationResult> RhostDisconnectedEvaluationResult = TaskUtilities.CreateCanceled<REvaluationResult>(new RHostDisconnectedException());
        private static readonly Task<ulong> RhostDisconnectedULongResult = TaskUtilities.CreateCanceled<ulong>(new RHostDisconnectedException());
        private static readonly Task<byte[]> RhostDisconnectedByteArrayResult = TaskUtilities.CreateCanceled<byte[]>(new RHostDisconnectedException());
        private static readonly Task<long> RhostDisconnectedLongResult = TaskUtilities.CreateCanceled<long>(new RHostDisconnectedException());

        public static IRContext TopLevelContext { get; } = new RContext(RContextType.TopLevel);
        
        private readonly IMessageTransport _transport;
        private readonly CancellationTokenSource _cts;
        private readonly IActionLog _log;
        private readonly ConcurrentDictionary<ulong, Request> _requests = new ConcurrentDictionary<ulong, Request>();

        private volatile Task _runTask;
        private volatile Task<REvaluationResult> _cancelEvaluationAfterRunTask;
        private volatile Task<ulong> _cancelCreateBlobAfterRunTask;
        private volatile Task<byte[]> _cancelBlobReadAfterRunTask;
        private volatile Task<long> _cancelBlobWriteOrSizeAfterRunTask;

        private int _rLoopDepth;
        private long _lastMessageId;
        private IRCallbacks _callbacks;

        private TaskCompletionSource<object> _cancelAllTcs;
        private CancellationTokenSource _cancelAllCts = new CancellationTokenSource();

        public RHost(string name, IRCallbacks callbacks, IMessageTransport transport, IActionLog log, CancellationTokenSource cts) {
            Check.ArgumentStringNullOrEmpty(nameof(name), name);

            _callbacks = callbacks;
            _transport = transport;
            _log = log;
            _cts = cts;
        }

        public void Dispose() {
            DisconnectAsync().DoNotWait();
        }

        public void FlushLog() {
            _log?.Flush();
        }

        private static Exception ProtocolError(FormattableString fs, object message = null) {
            var s = Invariant(fs);
            if (message != null) {
                s += "\n\n" + message;
            }
            Trace.Fail(s);
            return new InvalidDataException(s);
        }

        private async Task<Message> ReceiveMessageAsync(CancellationToken ct) {
            Message message;
            try {
                message = await _transport.ReceiveAsync(ct);
            } catch (MessageTransportException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            }

            if (message != null) {
                _log.Response(message.ToString(), _rLoopDepth);
            }

            return message;
        }

        private Message CreateMessage(string name, ulong requestId, JArray json, byte[] blob = null) {
            ulong id = (ulong)Interlocked.Add(ref _lastMessageId, 2);
            return new Message(id, requestId, name, json, blob);
        }

        private Message CreateRequestMessage(string name, JArray json, byte[] blob = null) {
            Debug.Assert(name.StartsWithOrdinal("?"));
            return CreateMessage(name, ulong.MaxValue, json, blob);
        }

        private async Task SendAsync(Message message, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            _log.Request(message.ToString(), _rLoopDepth);

            try {
                await _transport.SendAsync(message, ct);
            } catch (MessageTransportException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            } catch (MessageTransportException ex) {
                throw new RHostDisconnectedException(ex.Message, ex);
            }
        }

        private async Task<ulong> NotifyAsync(string name, CancellationToken ct, params object[] args) {
            Debug.Assert(name.StartsWithOrdinal("!"));
            TaskUtilities.AssertIsOnBackgroundThread();

            var message = CreateMessage(name, 0, new JArray(args));
            await SendAsync(message, ct);
            return message.Id;
        }

        private async Task<ulong> RespondAsync(Message request, CancellationToken ct, params object[] args) {
            Debug.Assert(request.Name.StartsWithOrdinal("?"));
            TaskUtilities.AssertIsOnBackgroundThread();

            var message = CreateMessage(":" + request.Name.Substring(1), request.Id, new JArray(args));
            await SendAsync(message, ct);
            return message.Id;
        }

        private static RContext[] GetContexts(Message message) {
            var contexts = message.GetArgument(0, "contexts", JTokenType.Array)
                .Select((token, i) => {
                    if (token.Type != JTokenType.Integer) {
                        throw ProtocolError($"Element #{i} of context array must be an integer:", message);
                    }
                    return new RContext((RContextType)(int)token);
                });
            return contexts.ToArray();
        }

        private void CancelAll() {
            var tcs = Volatile.Read(ref _cancelAllTcs);
            if (tcs != null) {
                Volatile.Write(ref _cancelAllCts, new CancellationTokenSource());
                tcs.TrySetResult(true);
            }
        }

        private async Task ShowDialog(Message request, MessageButtons buttons, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(2);
            var contexts = GetContexts(request);
            var s = request.GetString(1, "s", allowNull: true);

            MessageButtons input = await _callbacks.ShowDialog(contexts, s, buttons, ct);
            ct.ThrowIfCancellationRequested();

            string response;
            switch (input) {
                case MessageButtons.No:
                    response = "N";
                    break;
                case MessageButtons.Cancel:
                    response = "C";
                    break;
                case MessageButtons.Yes:
                    response = "Y";
                    break;
                default: {
                        FormattableString error = $"YesNoCancel: callback returned an invalid value: {input}";
                        Trace.Fail(Invariant(error));
                        throw new InvalidOperationException(Invariant(error));
                    }
            }

            await RespondAsync(request, ct, response);
        }

        private async Task ReadConsole(Message request, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(5);

            var contexts = GetContexts(request);
            var len = request.GetInt32(1, "len");
            var addToHistory = request.GetBoolean(2, "addToHistory");
            var retryReason = request.GetString(3, "retry_reason", allowNull: true);
            var prompt = request.GetString(4, "prompt", allowNull: true);

            string input = await _callbacks.ReadConsole(contexts, prompt, len, addToHistory, ct);
            ct.ThrowIfCancellationRequested();

            input = input.Replace("\r\n", "\n");
            await RespondAsync(request, ct, input);
        }

        public Task<ulong> CreateBlobAsync(CancellationToken cancellationToken) {
            if (_cancelCreateBlobAfterRunTask == null || _cancelCreateBlobAfterRunTask.IsCompleted) {
                return RhostDisconnectedULongResult;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<ulong>(cancellationToken);
            }

            return Task.WhenAny(CreateBlobAsyncWorker(cancellationToken), _cancelCreateBlobAfterRunTask).Unwrap();
        }

        private async Task<ulong> CreateBlobAsyncWorker(CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await CreateBlobRequest.CreateAsync(this, cancellationToken);
            return await request.Task;
        }

        public Task DestroyBlobsAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken) =>
            cancellationToken.IsCancellationRequested || _runTask == null || _runTask.IsCompleted
                ? Task.FromCanceled(new CancellationToken(true))
                : DestroyBlobsAsyncWorker(ids.ToArray(), cancellationToken);

        private async Task DestroyBlobsAsyncWorker(ulong[] ids, CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            await NotifyAsync("!DestroyBlob", cancellationToken, ids.Select(x => (object)x));
        }

        public Task<byte[]> BlobReadAllAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cancelBlobReadAfterRunTask == null || _cancelBlobReadAfterRunTask.IsCompleted) {
                return RhostDisconnectedByteArrayResult;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            return Task.WhenAny(BlobReadAllAsyncWorker(blobId, cancellationToken), _cancelBlobReadAfterRunTask).Unwrap();
        }

        private async Task<byte[]> BlobReadAllAsyncWorker(ulong blobId, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await BlobReadRequest.ReadAllAsync(this, blobId, ct);
            return await request.Task;
        }

        public Task<byte[]> BlobReadAsync(ulong blobId, long position, long count, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cancelBlobReadAfterRunTask == null || _cancelBlobReadAfterRunTask.IsCompleted) {
                return RhostDisconnectedByteArrayResult;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            return Task.WhenAny(BlobReadAsyncWorker(blobId, position, count, cancellationToken), _cancelBlobReadAfterRunTask).Unwrap();
        }

        private async Task<byte[]> BlobReadAsyncWorker(ulong blobId, long position, long count, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await BlobReadRequest.ReadAsync(this, blobId, position, count, ct);
            return await request.Task;
        }

        public Task<long> BlobWriteAsync(ulong blobId, byte[] data, long position, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cancelBlobWriteOrSizeAfterRunTask == null || _cancelBlobWriteOrSizeAfterRunTask.IsCompleted) {
                return RhostDisconnectedLongResult;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<long>(cancellationToken);
            }

            return Task.WhenAny(BlobWriteAsyncWorker(blobId, data, position, cancellationToken), _cancelBlobWriteOrSizeAfterRunTask).Unwrap();
        }

        private async Task<long> BlobWriteAsyncWorker(ulong blobId, byte[] data, long position, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await BlobWriteRequest.WriteAsync(this, blobId, data, position, ct);
            return await request.Task;
        }

        public Task<long> GetBlobSizeAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cancelBlobWriteOrSizeAfterRunTask == null || _cancelBlobWriteOrSizeAfterRunTask.IsCompleted) {
                return RhostDisconnectedLongResult;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<long>(cancellationToken);
            }

            return Task.WhenAny(GetBlobSizeAsyncWorker(blobId, cancellationToken), _cancelBlobWriteOrSizeAfterRunTask).Unwrap();
        }

        private async Task<long> GetBlobSizeAsyncWorker(ulong blobId, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await BlobSizeRequest.GetSizeAsync(this, blobId, ct);
            return await request.Task;
        }

        public Task<long> SetBlobSizeAsync(ulong blobId, long size, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cancelBlobWriteOrSizeAfterRunTask == null || _cancelBlobWriteOrSizeAfterRunTask.IsCompleted) {
                return RhostDisconnectedLongResult;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<long>(cancellationToken);
            }

            return Task.WhenAny(SetBlobSizeAsyncWorker(blobId, size, cancellationToken), _cancelBlobWriteOrSizeAfterRunTask).Unwrap();
        }

        private async Task<long> SetBlobSizeAsyncWorker(ulong blobId, long size, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await BlobSizeRequest.GetSizeAsync(this, blobId, ct);
            return await request.Task;
        }

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct) {
            if (_cancelEvaluationAfterRunTask == null) {
                throw new InvalidOperationException("Host was not started");
            } else if (_cancelEvaluationAfterRunTask.IsCompleted) { 
                return RhostDisconnectedEvaluationResult;
            }

            if (ct.IsCancellationRequested) {
                return Task.FromCanceled<REvaluationResult>(ct);
            }

            return Task.WhenAny(EvaluateAsyncWorker(expression, kind, ct), _cancelEvaluationAfterRunTask).Unwrap();
        }

        private async Task<REvaluationResult> EvaluateAsyncWorker(string expression, REvaluationKind kind, CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            var request = await EvaluationRequest.SendAsync(this, expression, kind, cancellationToken);
            return await request.Task;
        }

        /// <summary>
        /// Cancels any ongoing evaluations or interaction processing.
        /// </summary>
        public async Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_runTask == null) {
                // Nothing to cancel.
                return;
            }

            await TaskUtilities.SwitchToBackgroundThread();

            var tcs = new TaskCompletionSource<object>();
            if (Interlocked.CompareExchange(ref _cancelAllTcs, tcs, null) != null) {
                // Cancellation is already in progress - do nothing.
                return;
            }

            try {
                // Cancel any pending callbacks
                _cancelAllCts.Cancel();

                try {
                    await NotifyAsync("!//", CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token);
                } catch (OperationCanceledException) {
                    return;
                } catch (MessageTransportException) {
                    return;
                }

                await tcs.Task;
            } finally {
                Volatile.Write(ref _cancelAllTcs, null);
            }
        }

        public Task RequestShutdownAsync(bool saveRData) =>
            NotifyAsync("!Shutdown", _cts.Token, saveRData);

        public async Task DisconnectAsync() {
            if (_runTask == null) {
                return;
            }

            await TaskUtilities.SwitchToBackgroundThread();

            // We may get MessageTransportException from any concurrent SendAsync or ReceiveAsync when the host
            // drops connection after we request it to do so. To ensure that those don't bubble up to the
            // client, cancel this token to indicate that we're shutting down the host - SendAsync and
            // ReceiveAsync will take care of wrapping any WSE into OperationCanceledException.
            _cts.Cancel();

            try {
                // Don't use _cts, since it's already cancelled. We want to try to send this message in
                // any case, and we'll catch MessageTransportException if no-one is on the other end anymore.
                await _transport.CloseAsync();
            } catch (OperationCanceledException) {
            } catch (MessageTransportException) {
            }

            try {
                await _runTask;
            } catch (OperationCanceledException) {
                // Expected during disconnect.
            } catch (MessageTransportException) {
                // Possible and valid during disconnect.
            }
        }

        private async Task<Message> RunLoop(CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                _log.EnterRLoop(_rLoopDepth++);
                while (!ct.IsCancellationRequested) {
                    var message = await ReceiveMessageAsync(ct);
                    if (message == null) {
                        return null;
                    } else if (message.IsResponse) {
                        Request request;
                        if (!_requests.TryRemove(message.RequestId, out request)) {
                            throw ProtocolError($"Mismatched response - no request with such ID:", message);
                        } else if (message.Name != ":" + request.MessageName.Substring(1)) {
                            throw ProtocolError($"Mismatched response - message name does not match request '{request.MessageName}':", message);
                        }

                        request.Handle(this, message);
                        continue;
                    }

                    try {
                        switch (message.Name) {
                            case "!End":
                                message.ExpectArguments(1);
                                await _callbacks.Shutdown(message.GetBoolean(0, "rdataSaved"));
                                break;

                            case "!CanceledAll":
                                CancelAll();
                                break;

                            case "?YesNoCancel":
                                ShowDialog(message, MessageButtons.YesNoCancel, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?YesNo":
                                ShowDialog(message, MessageButtons.YesNo, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?OkCancel":
                                ShowDialog(message, MessageButtons.OKCancel, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?>":
                                ReadConsole(message, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "!":
                            case "!!":
                                message.ExpectArguments(1);
                                await _callbacks.WriteConsoleEx(
                                    message.GetString(0, "buf", allowNull: true),
                                    message.Name.Length == 1 ? OutputType.Output : OutputType.Error,
                                    ct);
                                break;

                            case "!ShowMessage":
                                message.ExpectArguments(1);
                                await _callbacks.ShowMessage(message.GetString(0, "s", allowNull: true), ct);
                                break;

                            case "!+":
                                await _callbacks.Busy(true, ct);
                                break;
                            case "!-":
                                await _callbacks.Busy(false, ct);
                                break;

                            case "!SetWD":
                                _callbacks.DirectoryChanged();
                                break;

                            case "!Library":
                                await _callbacks.ViewLibrary();
                                break;

                            case "!ShowFile":
                                message.ExpectArguments(3);
                                await _callbacks.ShowFile(
                                    message.GetString(0, "file"),
                                    message.GetString(1, "tabName"),
                                    message.GetBoolean(2, "delete.file"));
                                break;

                            case "!View":
                                message.ExpectArguments(2);
                                _callbacks.ViewObject(message.GetString(0, "x"), message.GetString(1, "title"));
                                break;

                            case "!Plot":
                                await _callbacks.Plot(
                                    new PlotMessage(
                                        Guid.Parse(message.GetString(0, "device_id")),
                                        Guid.Parse(message.GetString(1, "plot_id")),
                                        message.GetString(2, "file_path"),
                                        message.GetInt32(3, "device_num"),
                                        message.GetInt32(4, "active_plot_index"),
                                        message.GetInt32(5, "plot_count"),
                                        message.Blob),
                                    ct);
                                break;

                            case "?Locator":
                                var locatorResult = await _callbacks.Locator(Guid.Parse(message.GetString(0, "device_id")), ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, locatorResult.Clicked, locatorResult.X, locatorResult.Y);
                                break;

                            case "?PlotDeviceCreate":
                                var plotDeviceResult = await _callbacks.PlotDeviceCreate(Guid.Parse(message.GetString(0, "device_id")), ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, plotDeviceResult.Width, plotDeviceResult.Height, plotDeviceResult.Resolution);
                                break;

                            case "!PlotDeviceDestroy":
                                await _callbacks.PlotDeviceDestroy(Guid.Parse(message.GetString(0, "device_id")), ct);
                                break;

                            case "!WebBrowser":
                                await _callbacks.WebBrowser(message.GetString(0, "url"));
                                break;

                            case "!PackagesInstalled":
                                _callbacks.PackagesInstalled();
                                break;

                            case "!PackagesRemoved":
                                _callbacks.PackagesRemoved();
                                break;
                            case "!FetchFile":
                                var destPath = await _callbacks.SaveFileAsync(message.GetString(0, "file_path"), message.Blob);
                                await _callbacks.WriteConsoleEx(destPath, OutputType.Error, ct);
                                break;
                            default:
                                throw ProtocolError($"Unrecognized host message name:", message);
                        }
                    } catch (OperationCanceledException) when (!ct.IsCancellationRequested) {
                        // Canceled via _cancelAllCts - just move onto the next message.
                    }
                }
            } finally {
                _log.ExitRLoop(--_rLoopDepth);
            }

            return null;
        }

        private async Task RunWorker(CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            // Spin until the worker task is registered.
            while (_runTask == null) {
                await Task.Yield();
            }

            // Create cancellation tasks before proceeding with anything else, to avoid race conditions in usage of those tasks.
            _cancelEvaluationAfterRunTask = _runTask.ContinueWith(t => RhostDisconnectedEvaluationResult).Unwrap();
            _cancelCreateBlobAfterRunTask = _runTask.ContinueWith(t => RhostDisconnectedULongResult).Unwrap();
            _cancelBlobReadAfterRunTask = _runTask.ContinueWith(t => RhostDisconnectedByteArrayResult).Unwrap();
            _cancelBlobWriteOrSizeAfterRunTask = _runTask.ContinueWith(t => RhostDisconnectedLongResult).Unwrap();

            try {
                var message = await ReceiveMessageAsync(ct);
                if (message == null || !message.IsNotification || message.Name != "!Microsoft.R.Host") {
                    throw ProtocolError($"Microsoft.R.Host handshake expected:", message);
                }

                var protocolVersion = message.GetInt32(0, "protocol_version");
                if (protocolVersion != 1) {
                    throw ProtocolError($"Unsupported RHost protocol version:", message);
                }

                var rVersion = message.GetString(1, "R_version");
                await _callbacks.Connected(rVersion);

                message = await RunLoop(ct);
                if (message != null) {
                    throw ProtocolError($"Unexpected host response message:", message);
                }
            } finally {
                // Signal cancellation to any callbacks that haven't returned yet.
                _cts.Cancel();

                await _callbacks.Disconnected();
            }
        }

        public async Task Run(CancellationToken ct = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (_runTask != null) {
                throw new InvalidOperationException("This host is already running.");
            }

            ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;

            try {
                _runTask = RunWorker(ct);
                await _runTask;
            } catch (OperationCanceledException) when (ct.IsCancellationRequested) {
                // Expected cancellation, do not propagate, just exit process
            } catch (MessageTransportException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            } catch (Exception ex) {
                var message = "Exception in RHost run loop:\n" + ex;
                _log.WriteLineAsync(LogVerbosity.Minimal, MessageCategory.Error, message).DoNotWait();
                Debug.Fail(message);
                throw;
            } finally {
                // Signal cancellation to any callbacks that haven't returned yet.
                _cts.Cancel();

                _requests.Clear();
            }
        }

        public void DetachCallback() {
            Interlocked.Exchange(ref _callbacks, new NullRCallbacks());
        }

        internal Task GetRHostRunTask() => _runTask;
    }
}
