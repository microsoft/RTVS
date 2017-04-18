// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public sealed partial class RHost : IDisposable, IRExpressionEvaluator, IRBlobService {
        public static IRContext TopLevelContext { get; } = new RContext(RContextType.TopLevel);

        public string Name { get; }

        private readonly IMessageTransport _transport;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentDictionary<ulong, Request> _requests = new ConcurrentDictionary<ulong, Request>();
        private readonly BinaryAsyncLock _disconnectLock = new BinaryAsyncLock();

        private volatile Task _runTask;

        private int _rLoopDepth;
        private long _lastMessageId;
        private IRCallbacks _callbacks;

        private TaskCompletionSource<object> _cancelAllTcs;
        private CancellationTokenSource _cancelAllCts = new CancellationTokenSource();

        public RHost(string name, IRCallbacks callbacks, IMessageTransport transport, IActionLog log) {
            Check.ArgumentStringNullOrEmpty(nameof(name), name);

            Name = name;
            _callbacks = callbacks;
            _transport = transport;
            Log = log;
            _cts = new CancellationTokenSource();
            _cts.Token.Register(() => { Log.RHostProcessExited(); });
        }

        public IActionLog Log { get; }

        public void Dispose() {
            DisconnectAsync().DoNotWait();
        }

        public void FlushLog() {
            Log?.Flush();
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
                Log.Response(message.ToString(), _rLoopDepth);
            } else {
                Log.Response(_transport.CloseStatusDescription, _rLoopDepth);
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

            Log.Request(message.ToString(), _rLoopDepth);

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
            tcs?.TrySetResult(true);
        }

        private async Task ShowLocalizedDialogFormat(Message request, MessageButtons buttons, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var response = await ShowDialog(new RContext[0], GetLocalizedString(request), buttons, ct);
            await RespondAsync(request, ct, response);
        }

        private async Task ShowDialog(Message request, MessageButtons buttons, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(2);
            var contexts = GetContexts(request);
            var s = request.GetString(1, "s", allowNull: true);

            var response = await ShowDialog(contexts, s, buttons, ct);
            await RespondAsync(request, ct, response);
        }

        private async Task<string> ShowDialog(RContext[] contexts, string message, MessageButtons buttons, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            MessageButtons input = await _callbacks.ShowDialog(contexts, message, buttons, ct);
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
                        var error = Invariant($"YesNoCancel: callback returned an invalid value: {input}");
                        Trace.Fail(error);
                        throw new InvalidOperationException(error);
                    }
            }
            return response;
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

        public async Task<ulong> CreateBlobAsync(CancellationToken cancellationToken) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await CreateBlobRequest.CreateAsync(this, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
        }

        public Task DestroyBlobsAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken) =>
            cancellationToken.IsCancellationRequested || _runTask == null || _runTask.IsCompleted
                ? Task.FromCanceled(new CancellationToken(true))
                : DestroyBlobsAsyncWorker(ids.ToArray(), cancellationToken);

        private async Task DestroyBlobsAsyncWorker(ulong[] ids, CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            await NotifyAsync("!DestroyBlob", cancellationToken, ids.Select(x => (object)x));
        }

        public async Task<byte[]> BlobReadAllAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await BlobReadRequest.ReadAllAsync(this, blobId, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
        }

        public async Task<byte[]> BlobReadAsync(ulong blobId, long position, long count, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await BlobReadRequest.ReadAsync(this, blobId, position, count, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
        }

        public async Task<long> BlobWriteAsync(ulong blobId, byte[] data, long position, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await BlobWriteRequest.WriteAsync(this, blobId, data, position, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
        }

        public async Task<long> GetBlobSizeAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await BlobSizeRequest.GetSizeAsync(this, blobId, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
        }

        public async Task<long> SetBlobSizeAsync(ulong blobId, long size, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await BlobSizeRequest.GetSizeAsync(this, blobId, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
        }

        public async Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken cancellationToken) {
            if (_runTask == null) {
                throw new InvalidOperationException("Host was not started");
            }

            using (CancellationTokenUtilities.Link(ref cancellationToken, _cts.Token)) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var request = await EvaluationRequest.SendAsync(this, expression, kind, cancellationToken);
                    return await request.Task;
                } catch (OperationCanceledException ex) when (_cts.IsCancellationRequested) {
                    throw new RHostDisconnectedException(Resources.Error_RHostIsStopped, ex);
                }
            }
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

            using (tcs.RegisterForCancellation(_cts.Token))
            using (tcs.RegisterForCancellation(cancellationToken)) {
                try {
                    // Cancel any pending callbacks
                    _cancelAllCts.Cancel();
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
                    try {
                        await NotifyAsync("!//", cts.Token);
                    } catch (OperationCanceledException) {
                        return;
                    } catch (MessageTransportException) {
                        return;
                    } finally {
                        cts.Dispose();
                    }

                    await tcs.Task;
                } finally {
                    Volatile.Write(ref _cancelAllTcs, null);
                }
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
            var token = await _disconnectLock.WaitAsync();
            if (!token.IsSet) {
                try {
                    // Don't use _cts, since it's already cancelled. We want to try to send this message in
                    // any case, and we'll catch MessageTransportException if no-one is on the other end anymore.
                    await _transport.CloseAsync();
                    token.Set();
                } catch (Exception ex) when (ex is OperationCanceledException || ex is MessageTransportException) {
                    token.Set();
                } finally {
                    token.Reset();
                }
            }

            try {
                await _runTask;
            } catch (OperationCanceledException) {
                // Expected during disconnect.
            } catch (MessageTransportException) {
                // Possible and valid during disconnect.
            }
        }

        private async Task<Message> RunLoop(CancellationToken loopCt) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var cancelAllCtsLink = CancellationTokenSource.CreateLinkedTokenSource(loopCt, _cancelAllCts.Token);
            var ct = cancelAllCtsLink.Token;
            try {
                Log.EnterRLoop(_rLoopDepth++);
                while (!loopCt.IsCancellationRequested) {
                    var message = await ReceiveMessageAsync(loopCt);
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
                                ShowDialog(message, MessageButtons.YesNoCancel, ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?YesNo":
                                ShowDialog(message, MessageButtons.YesNo, ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?OkCancel":
                                ShowDialog(message, MessageButtons.OKCancel, ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?LocYesNoCancel":
                                ShowLocalizedDialogFormat(message, MessageButtons.YesNoCancel, ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?LocYesNo":
                                ShowLocalizedDialogFormat(message, MessageButtons.YesNo, ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?LocOkCancel":
                                ShowLocalizedDialogFormat(message, MessageButtons.OKCancel, ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "?>":
                                ReadConsole(message, ct)
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
                                await _callbacks.ViewLibrary(ct);
                                break;

                            case "!ShowFile":
                                message.ExpectArguments(3);
                                // Do not await since it blocks callback from calling the host again
                                _callbacks.ShowFile(
                                    message.GetString(0, "file"),
                                    message.GetString(1, "tabName"),
                                    message.GetBoolean(2, "delete.file"),
                                    ct).DoNotWait();
                                break;

                            case "?EditFile":
                                message.ExpectArguments(2);
                                // Opens file in editor and blocks until file is closed.
                                var content = await _callbacks.EditFileAsync(message.GetString(0, "name", allowNull: true), message.GetString(1, "file", allowNull: true), ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, content);
                                break;

                            case "!View":
                                message.ExpectArguments(2);
                                _callbacks.ViewObject(message.GetString(0, "x"), message.GetString(1, "title"), ct)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
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
                                _callbacks.WebBrowser(message.GetString(0, "url"), ct)
                                    .DoNotWait();
                                break;

                            case "?BeforePackagesInstalled":
                                await _callbacks.BeforePackagesInstalledAsync(ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, true);
                                break;

                            case "?AfterPackagesInstalled":
                                await _callbacks.AfterPackagesInstalledAsync(ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, true);
                                break;

                            case "!PackagesRemoved":
                                _callbacks.PackagesRemoved();
                                break;

                            case "!FetchFile":
                                var remoteFileName = message.GetString(0, "file_remote_name");
                                var remoteBlobId = message.GetUInt64(1, "blob_id");
                                var localPath = message.GetString(2, "file_local_path");
                                Task.Run(async () => {
                                    var destPath = await _callbacks.FetchFileAsync(remoteFileName, remoteBlobId, localPath, ct);
                                    if (!message.GetBoolean(3, "silent")) {
                                        await _callbacks.WriteConsoleEx(destPath, OutputType.Error, ct);
                                    }
                                }).DoNotWait();
                                break;

                            case "!LocMessage":
                                _callbacks.WriteConsoleEx(GetLocalizedString(message) + Environment.NewLine, OutputType.Output, ct).DoNotWait();
                                break;

                            case "!LocWarning":
                                _callbacks.WriteConsoleEx(GetLocalizedString(message) + Environment.NewLine, OutputType.Error, ct).DoNotWait();
                                break;

                            default:
                                throw ProtocolError($"Unrecognized host message name:", message);
                        }

                        if (_cancelAllCts.IsCancellationRequested) {
                            ct = UpdateCancelAllCtsLink(ref cancelAllCtsLink, loopCt);
                        }
                    } catch (OperationCanceledException) when (_cancelAllCts.IsCancellationRequested) {
                        // Canceled via _cancelAllCts - update cancelAllCtsLink and move on
                        ct = UpdateCancelAllCtsLink(ref cancelAllCtsLink, loopCt);
                    }
                }
            } finally {
                // asyncronously-running handlers like ReadConsole and ShowDialog that were started in the loop should be canceled
                cancelAllCtsLink.Cancel();
                cancelAllCtsLink.Dispose();
                Log.ExitRLoop(--_rLoopDepth);
            }

            return null;
        }

        private string GetLocalizedString(Message message) {
            var s = _callbacks.GetLocalizedString(message.GetString(0, "id"));
            if (message.ArgumentCount == 2) {
                var args = message.GetArgument(1, "a", JTokenType.Array).Select(o => o.Value<object>()).ToArray();
                s = string.Format(CultureInfo.CurrentCulture, s, args);
            }
            return s;
        }

        private CancellationToken UpdateCancelAllCtsLink(ref CancellationTokenSource cancelAllCtsLink, CancellationToken loopCt) {
            cancelAllCtsLink.Dispose();
            Interlocked.Exchange(ref _cancelAllCts, new CancellationTokenSource());
            cancelAllCtsLink = CancellationTokenSource.CreateLinkedTokenSource(loopCt, _cancelAllCts.Token);
            return cancelAllCtsLink.Token;
        }

        private async Task RunWorker(CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            // Spin until the worker task is registered.
            while (_runTask == null) {
                await Task.Yield();
            }

            try {
                var message = await ReceiveMessageAsync(ct);
                if (message == null) {
                    // Socket is closed before connection is established. Just exit.
                    return;
                }

                if (!message.IsNotification || message.Name != "!Microsoft.R.Host") {
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

        public async Task Run(CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (_runTask != null) {
                throw new InvalidOperationException("This host is already running.");
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

            try {
                _runTask = RunWorker(cts.Token);
                await _runTask;
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || _cts.Token.IsCancellationRequested) {
                // Expected cancellation, do not propagate, just exit process
            } catch (MessageTransportException ex) when (cancellationToken.IsCancellationRequested || _cts.Token.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            } catch (Exception ex) {
                var message = "Exception in RHost run loop:\n" + ex;
                Log.WriteLine(LogVerbosity.Minimal, MessageCategory.Error, message);
                Debug.Fail(message);
                throw;
            } finally {
                cts.Dispose();

                // Signal cancellation to any callbacks that haven't returned yet.
                _cts.Cancel();

                _requests.Clear();
            }
        }

        public void DetachCallback() {
            Interlocked.Exchange(ref _callbacks, new NullRCallbacks());
        }

        public Task GetRHostRunTask() => _runTask;
    }
}
