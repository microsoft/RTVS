// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public sealed partial class RHost : IDisposable, IRExpressionEvaluator {
        private readonly string[] parseStatusNames = { "NULL", "OK", "INCOMPLETE", "ERROR", "EOF" };

        public const int DefaultPort = 5118;
        public const string RHostExe = "Microsoft.R.Host.exe";
        public const string RBinPathX64 = @"bin\x64";

        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif

        public static IRContext TopLevelContext { get; } = new RContext(RContextType.TopLevel);

        private static bool showConsole = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RTVS_HOST_CONSOLE"));

        private IMessageTransport _transport;
        private readonly object _transportLock = new object();
        private readonly TaskCompletionSource<IMessageTransport> _transportTcs = new TaskCompletionSource<IMessageTransport>();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _name;
        private readonly IRCallbacks _callbacks;
        private readonly LinesLog _log;
        private readonly FileLogWriter _fileLogWriter;
        private Process _process;
        private volatile Task _runTask;
        private int _rLoopDepth;
        private long _lastMessageId = -1;
        private readonly ConcurrentDictionary<string, EvaluationRequest> _evalRequests = new ConcurrentDictionary<string, EvaluationRequest>();

        private TaskCompletionSource<object> _cancelAllTcs;
        private CancellationTokenSource _cancelAllCts = new CancellationTokenSource();

        public int? ProcessId => _process?.Id;

        public RHost(string name, IRCallbacks callbacks) {
            Check.ArgumentStringNullOrEmpty(nameof(name), name);

            _callbacks = callbacks;
            _name = name;

            _fileLogWriter = FileLogWriter.InTempFolder("Microsoft.R.Host.Client" + "_" + name);
            _log = new LinesLog(_fileLogWriter);
        }

        public void Dispose() {
            _cts.Cancel();
        }

        public void FlushLog() {
            _fileLogWriter?.Flush();
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
            var sb = new StringBuilder();

            string json;
            try {
                json = await _transport.ReceiveAsync(ct);
            } catch (MessageTransportException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            }

            _log.Response(json, _rLoopDepth);

            var token = Json.ParseToken(json);

            var value = token as JValue;
            if (value != null && value.Value == null) {
                return null;
            }

            return new Message(token);
        }

        private JArray CreateMessage(out string id, string name, params object[] args) {
            long n = Interlocked.Add(ref _lastMessageId, 2);
            id = "#" + n + "#";
            return new JArray(id, name, args);
        }

        private async Task SendAsync(JToken token, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var json = JsonConvert.SerializeObject(token);
            _log.Request(json, _rLoopDepth);

            try {
                await _transport.SendAsync(json, ct);
            } catch (MessageTransportException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            }
        }

        private async Task<string> SendAsync(string name, CancellationToken ct, params object[] args) {
            string id;
            var message = CreateMessage(out id, name, args);
            await SendAsync(message, ct);
            return id;
        }

        private async Task<string> RespondAsync(Message request, CancellationToken ct, params object[] args) {
            TaskUtilities.AssertIsOnBackgroundThread();

            string id;
            var message = CreateMessage(out id, ":", request.Id, request.Name, args);
            await SendAsync(message, ct);
            return id;
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

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct) {
            return ct.IsCancellationRequested || _runTask == null || _runTask.IsCompleted
                ? Task.FromCanceled<REvaluationResult>(new CancellationToken(true))
                : EvaluateAsyncBackground(expression, kind, ct);
        }

        private async Task<REvaluationResult> EvaluateAsyncBackground(string expression, REvaluationKind kind, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            JArray message;
            var request = new EvaluationRequest(this, expression, kind, out message);
            _evalRequests[request.Id] = request;

            await SendAsync(message, ct);
            return await request.CompletionSource.Task;
        }

        private void ProcessEvaluationResult(Message response) {
            EvaluationRequest request; ;
            if (!_evalRequests.TryRemove(response.RequestId, out request)) {
                throw ProtocolError($"Unexpected response to evaluation request {response.RequestId} that is not pending.");
            }

            response.ExpectArguments(1, 3);
            var firstArg = response[0] as JValue;
            if (firstArg != null && firstArg.Value == null) {
                request.CompletionSource.SetCanceled();
            }

            if (response.Name != request.MessageName) {
                throw ProtocolError($"Mismatched host response ['{response.Id}',':','{response.Name}',...] to evaluation request ['{request.Id}','{request.MessageName}','{request.Expression}']");
            }

            response.ExpectArguments(3);
            var parseStatus = response.GetEnum<RParseStatus>(0, "parseStatus", parseStatusNames);
            var error = response.GetString(1, "error", allowNull: true);

            REvaluationResult result;
            if (request.Kind.HasFlag(REvaluationKind.NoResult)) {
                result = new REvaluationResult(error, parseStatus);
            } else {
                result = new REvaluationResult(response[2], error, parseStatus);
            }
            request.CompletionSource.SetResult(result);
        }

        /// <summary>
        /// Cancels any ongoing evaluations or interaction processing.
        /// </summary>
        public async Task CancelAllAsync() {
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
                    await SendAsync("/", _cts.Token, null);
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

        public async Task DisconnectAsync() {
            if (_runTask == null) {
                throw new InvalidOperationException("Not connected to host.");
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
                await SendAsync(JValue.CreateNull(), new CancellationToken());
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
                    } else if (message.RequestId != null) {
                        if (message.Name.StartsWithIgnoreCase("=")) {
                            ProcessEvaluationResult(message);
                            continue;
                        } else {
                            throw ProtocolError($"Unrecognized host response message name:", message);
                        }
                    }

                    try {
                        switch (message.Name) {
                            case "\\":
                                CancelAll();
                                break;

                            case "?":
                                ShowDialog(message, MessageButtons.YesNoCancel, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "??":
                                ShowDialog(message, MessageButtons.YesNo, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case "???":
                                ShowDialog(message, MessageButtons.OKCancel, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token)
                                    .SilenceException<MessageTransportException>()
                                    .DoNotWait();
                                break;

                            case ">":
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

                            case "![]":
                                message.ExpectArguments(1);
                                await _callbacks.ShowMessage(message.GetString(0, "s", allowNull: true), ct);
                                break;

                            case "~+":
                                await _callbacks.Busy(true, ct);
                                break;
                            case "~-":
                                await _callbacks.Busy(false, ct);
                                break;

                            case "setwd":
                                _callbacks.DirectoryChanged();
                                break;

                            case "library":
                                await _callbacks.ViewLibrary();
                                break;

                            case "show_file":
                                message.ExpectArguments(3);
                                await _callbacks.ShowFile(
                                    message.GetString(0, "file"),
                                    message.GetString(1, "tabName"),
                                    message.GetBoolean(2, "delete.file"));
                                break;

                            case "View":
                                message.ExpectArguments(2);
                                _callbacks.ViewObject(message.GetString(0, "x"), message.GetString(1, "title"));
                                break;

                            case "Plot":
                                await _callbacks.Plot(
                                    new PlotMessage(
                                        Guid.Parse(message.GetString(0, "device_id")),
                                        Guid.Parse(message.GetString(1, "plot_id")),
                                        message.GetString(2, "file_path"),
                                        message.GetInt32(3, "device_num"),
                                        message.GetInt32(4, "active_plot_index"),
                                        message.GetInt32(5, "plot_count")),
                                    ct);
                                break;

                            case "Locator":
                                var locatorResult = await _callbacks.Locator(Guid.Parse(message.GetString(0, "device_id")), ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, locatorResult.Clicked, locatorResult.X, locatorResult.Y);
                                break;

                            case "PlotDeviceCreate":
                                var plotDeviceResult = await _callbacks.PlotDeviceCreate(Guid.Parse(message.GetString(0, "device_id")), ct);
                                ct.ThrowIfCancellationRequested();
                                await RespondAsync(message, ct, plotDeviceResult.Width, plotDeviceResult.Height, plotDeviceResult.Resolution);
                                break;

                            case "PlotDeviceDestroy":
                                await _callbacks.PlotDeviceDestroy(Guid.Parse(message.GetString(0, "device_id")), ct);
                                break;

                            case "open_url":
                                await _callbacks.Browser(message.GetString(0, "help_url"));
                                break;

                            default:
                                throw ProtocolError($"Unrecognized host message name:", message);
                        }
                    } catch (OperationCanceledException) when (!ct.IsCancellationRequested) {
                        // Cancelled via _cancelAllCts - just move onto the next message.
                    }
                }
            } finally {
                _log.ExitRLoop(--_rLoopDepth);
            }

            return null;
        }

        private async Task RunWorker(CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                var message = await ReceiveMessageAsync(ct);
                if (message.Name != "Microsoft.R.Host" || message.RequestId != null) {
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
                await _callbacks.Disconnected();
            }
        }

        public async Task Run(IMessageTransport transport, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (_runTask != null) {
                throw new InvalidOperationException("This host is already running.");
            }

            if (transport != null) {
                lock (_transportLock) {
                    _transport = transport;
                }
            } else if (_transport == null) {
                throw new ArgumentNullException(nameof(transport));
            }

            try {
                await (_runTask = RunWorker(ct));
            } catch (OperationCanceledException) when (ct.IsCancellationRequested) {
                // Expected cancellation, do not propagate, just exit process
            } catch (MessageTransportException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            } catch (Exception ex) {
                Trace.Fail("Exception in RHost run loop:\n" + ex);
                throw;
            }
        }

        private WebSocketMessageTransport CreateWebSocketMessageTransport() {
            lock (_transportLock) {
                if (_transport != null) {
                    throw new MessageTransportException("More than one incoming connection.");
                }

                var transport = new WebSocketMessageTransport();
                _transportTcs.SetResult(_transport = transport);
                return transport;
            }
        }

        public async Task CreateAndRun(string rHome, string rhostDirectory = null, string rCommandLineArguments = null, int timeout = 3000, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            rhostDirectory = rhostDirectory ?? Path.GetDirectoryName(typeof(RHost).Assembly.GetAssemblyPath());
            rCommandLineArguments = rCommandLineArguments ?? string.Empty;

            string rhostExe = Path.Combine(rhostDirectory, RHostExe);
            string rBinPath = Path.Combine(rHome, RBinPathX64);

            if (!File.Exists(rhostExe)) {
                throw new RHostBinaryMissingException();
            }

            // Grab an available port from the ephemeral port range (per RFC 6335 8.1.2) for the server socket.

            WebSocketServer server = null;
            var rnd = new Random();
            const int ephemeralRangeStart = 49152;
            var ports =
                from port in Enumerable.Range(ephemeralRangeStart, 0x10000 - ephemeralRangeStart)
                let pos = rnd.NextDouble()
                orderby pos
                select port;

            foreach (var port in ports) {
                ct.ThrowIfCancellationRequested();

                server = new WebSocketServer(IPAddress.Loopback, port) {
                    ReuseAddress = false,
                    WaitTime = HeartbeatTimeout,
                };
                server.AddWebSocketService("/", CreateWebSocketMessageTransport);

                try {
                    server.Start();
                    break;
                } catch (SocketException ex) {
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) {
                        server = null;
                    } else {
                        throw new MessageTransportException(ex);
                    }
                } catch (WebSocketException ex) {
                    throw new MessageTransportException(ex);
                }
            }

            if (server == null) {
                throw new MessageTransportException(new SocketException((int)SocketError.AddressAlreadyInUse));
            }

            var psi = new ProcessStartInfo {
                FileName = rhostExe,
                UseShellExecute = false
            };

            var shortHome = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(rHome, shortHome, shortHome.Capacity);
            psi.EnvironmentVariables["R_HOME"] = shortHome.ToString();

            psi.EnvironmentVariables["PATH"] = rBinPath + ";" + Environment.GetEnvironmentVariable("PATH");

            if (_name != null) {
                psi.Arguments += " --rhost-name " + _name;
            }

            psi.Arguments += Invariant($" --rhost-connect ws://127.0.0.1:{server.Port}");

            if (!showConsole) {
                psi.CreateNoWindow = true;
            }

            if (!string.IsNullOrWhiteSpace(rCommandLineArguments)) {
                psi.Arguments += Invariant($" {rCommandLineArguments}");
            }

            using (this)
            using (_process = Process.Start(psi)) {
                _log.RHostProcessStarted(psi);
                _process.EnableRaisingEvents = true;
                _process.Exited += delegate { Dispose(); };

                try {
                    ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;

                    // Timeout increased to allow more time in test and code coverage runs.
                    await Task.WhenAny(_transportTcs.Task, Task.Delay(timeout)).Unwrap();
                    if (!_transportTcs.Task.IsCompleted) {
                        _log.FailedToConnectToRHost();
                        throw new RHostTimeoutException("Timed out waiting for R host process to connect");
                    }

                    await Run(null, ct);
                } catch (Exception) {
                    // TODO: delete when we figure out why host occasionally times out in code coverage runs.
                    //await _log.WriteFormatAsync(MessageCategory.Error, "Exception running R Host: {0}", ex.Message);
                    throw;
                } finally {
                    if (!_process.HasExited) {
                        try {
                            _process.WaitForExit(500);
                            if (!_process.HasExited) {
                                _process.Kill();
                                _process.WaitForExit();
                            }
                        } catch (InvalidOperationException) {
                        }
                    }
                    _log.RHostProcessExited();
                }
            }
        }

        internal Task GetRHostRunTask() => _runTask;
    }
}
