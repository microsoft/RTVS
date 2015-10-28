using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Actions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public sealed partial class RHost : IDisposable, IRExpressionEvaluator {
        private readonly string[] parseStatusNames = { "NULL", "OK", "INCOMPLETE", "ERROR", "EOF" };

        public const int DefaultPort = 5118;
        public const string RHostExe = "Microsoft.R.Host.exe";
        public const string RBinPathX64 = @"bin\x64";

        public static IRContext TopLevelContext { get; } = new RContext(RContextType.TopLevel);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IRCallbacks _callbacks;
        private readonly LinesLog _log;
        private readonly FileLogWriter _fileLogWriter;
        private Process _process;
        private volatile Task _runTask;

        private ClientWebSocket _socket;
        private readonly byte[] _buffer = new byte[0x100000];
        private readonly SemaphoreSlim
            _socketSendLock = new SemaphoreSlim(1, 1),      // for _socket.SendAsync
            _socketReceiveLock = new SemaphoreSlim(1, 1);   // for _socket.ReceiveAsync and _buffer

        private bool _canEval;
        private int _rLoopDepth;
        private long _nextMessageId = 1;

        private TaskCompletionSource<object> _cancelAllTcs;
        private CancellationTokenSource _cancelAllCts = new CancellationTokenSource();

        public RHost(IRCallbacks callbacks) {
            _callbacks = callbacks;
            _fileLogWriter = FileLogWriter.InTempFolder("Microsoft.R.Host.Client");
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

            await _socketReceiveLock.WaitAsync(ct);
            try {
                WebSocketReceiveResult wsrr;
                do {
                    wsrr = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), ct);
                    if (wsrr.CloseStatus != null) {
                        return null;
                    }
                    sb.Append(Encoding.UTF8.GetString(_buffer, 0, wsrr.Count));
                } while (!wsrr.EndOfMessage);
            } catch (WebSocketException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            } finally {
                _socketReceiveLock.Release();
            }

            var json = sb.ToString();
            _log.Response(json, _rLoopDepth);

            var token = JToken.Parse(json);

            var value = token as JValue;
            if (value != null && value.Value == null) {
                return null;
            }

            return new Message(token);
        }

        private JArray CreateMessage(CancellationToken ct, out string id, string name, params object[] args) {
            id = "#" + _nextMessageId + "#";
            _nextMessageId += 2;
            return new JArray(id, name, args);
        }

        private async Task SendAsync(JToken token, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var json = JsonConvert.SerializeObject(token);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await _socketSendLock.WaitAsync(ct);
            try {
                await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, ct);
            } catch (WebSocketException ex) when (ct.IsCancellationRequested) {
                // Network errors during cancellation are expected, but should not be exposed to clients.
                throw new OperationCanceledException(new OperationCanceledException().Message, ex);
            } finally {
                _socketSendLock.Release();
            }

            _log.Request(json, _rLoopDepth);
        }

        private async Task<string> SendAsync(string name, CancellationToken ct, params object[] args) {
            string id;
            var message = CreateMessage(ct, out id, name, args);
            await SendAsync(message, ct);
            return id;
        }

        private async Task<string> RespondAsync(Message request, CancellationToken ct, params object[] args) {
            TaskUtilities.AssertIsOnBackgroundThread();

            string id;
            var message = CreateMessage(ct, out id, ":", request.Id, request.Name, args);
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

        private async Task YesNoCancel(Message request, bool allowEval, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(2);
            var contexts = GetContexts(request);
            var s = request.GetString(1, "s", allowNull: true);

            YesNoCancel input;
            try {
                _canEval = allowEval;
                input = await _callbacks.YesNoCancel(contexts, s, _canEval, ct);
            } finally {
                _canEval = false;
            }

            ct.ThrowIfCancellationRequested();

            string response;
            switch (input) {
                case Client.YesNoCancel.No:
                    response = "N";
                    break;
                case Client.YesNoCancel.Cancel:
                    response = "C";
                    break;
                case Client.YesNoCancel.Yes:
                    response = "Y";
                    break;
                default:
                    {
                        FormattableString error = $"YesNoCancel: callback returned an invalid value: {input}";
                        Trace.Fail(Invariant(error));
                        throw new InvalidOperationException(Invariant(error));
                    }
            }

            await RespondAsync(request, ct, response);
        }

        private async Task ReadConsole(Message request, bool allowEval, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(5);

            var contexts = GetContexts(request);
            var len = request.GetInt32(1, "len");
            var addToHistory = request.GetBoolean(2, "addToHistory");
            var retryReason = request.GetString(3, "retry_reason", allowNull: true);
            var prompt = request.GetString(4, "prompt", allowNull: true);

            string input;
            try {
                _canEval = allowEval;
                input = await _callbacks.ReadConsole(contexts, prompt, len, addToHistory, _canEval, ct);
            } finally {
                _canEval = false;
            }

            ct.ThrowIfCancellationRequested();

            input = input.Replace("\r\n", "\n");
            await RespondAsync(request, ct, input);
        }

        public async Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (!_canEval) {
                throw new InvalidOperationException("EvaluateAsync can only be called while ReadConsole or YesNoCancel is pending.");
            }

            bool reentrant = false, jsonResult = false;

            var nameBuilder = new StringBuilder("=");
            if (kind.HasFlag(REvaluationKind.Reentrant)) {
                nameBuilder.Append('@');
                reentrant = true;
            }
            if (kind.HasFlag(REvaluationKind.Cancelable)) {
                nameBuilder.Append('/');
                reentrant = true;
            }
            if (kind.HasFlag(REvaluationKind.Json)) {
                nameBuilder.Append('j');
                jsonResult = true;
            }
            if (kind.HasFlag(REvaluationKind.BaseEnv)) {
                nameBuilder.Append('B');
            }
            if (kind.HasFlag(REvaluationKind.EmptyEnv)) {
                nameBuilder.Append('E');
            }
            var name = nameBuilder.ToString();

            _canEval = false;
            try {
                expression = expression.Replace("\r\n", "\n");
                var id = await SendAsync(name, ct, expression);

                var response = await RunLoop(ct, reentrant);
                if (response.RequestId != id || response.Name != name) {
                    throw ProtocolError($"Mismatched host response ['{response.Id}',':','{response.Name}',...] to evaluation request ['{id}','{name}','{expression}']");
                }

                response.ExpectArguments(1, 3);
                var firstArg = response[0] as JValue;
                if (firstArg != null && firstArg.Value == null) {
                    throw new OperationCanceledException(Invariant($"Evaluation canceled: {expression}"));
                }

                response.ExpectArguments(3);
                var parseStatus = response.GetEnum<RParseStatus>(0, "parseStatus", parseStatusNames);
                var error = response.GetString(1, "error", allowNull: true);

                if (jsonResult) {
                    return new REvaluationResult(response[2], error, parseStatus);
                } else {
                    return new REvaluationResult(response.GetString(2, "value", allowNull: true), error, parseStatus);
                }
            } finally {
                _canEval = true;
            }
        }

        /// <summary>
        /// Cancels any ongoing evaluations or interaction processing.
        /// </summary>
        public async Task CancelAllAsync() {
            if (_runTask == null) {
                throw new InvalidOperationException("Not connected to host.");
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
                } catch (WebSocketException) {
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

            // We may get a WebSocketException from any concurrent SendAsync or ReceiveAsync when the host
            // drops connection after we request it to do so. To ensure that those don't bubble up to the
            // client, cancel this token to indicate that we're shutting down the host - SendAsync and
            // ReceiveAsync will take care of wrapping any WSE into OperationCanceledException.
            _cts.Cancel();

            try {
                // Don't use _cts, since it's already cancelled. We want to try to send this message in
                // any case, and we'll catch WebSocketException if no-one is on the other end anymore.
                await SendAsync(JValue.CreateNull(), new CancellationToken());
            } catch (OperationCanceledException) {
            } catch (WebSocketException) {
            }

            try {
                await _runTask;
            } catch (OperationCanceledException) {
                // Expected during disconnect.
            }
        }

        private async Task<Message> RunLoop(CancellationToken ct, bool allowEval) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                _log.EnterRLoop(_rLoopDepth++);
                while (!ct.IsCancellationRequested) {
                    var message = await ReceiveMessageAsync(ct);
                    if (message == null) {
                        return null;
                    } else if (message.RequestId != null) {
                        return message;
                    }

                    try {
                        switch (message.Name) {
                            case "\\":
                                CancelAll();
                                break;

                            case "?":
                                await YesNoCancel(message, allowEval, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token);
                                break;

                            case ">":
                                await ReadConsole(message, allowEval, CancellationTokenSource.CreateLinkedTokenSource(ct, _cancelAllCts.Token).Token);
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

                            case "PlotXaml":
                                await _callbacks.PlotXaml(message.GetString(0, "xaml_file_path"), ct);
                                // TODO: delete temporary xaml and bitmap files
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

                message = await RunLoop(ct, allowEval: true);
                if (message != null) {
                    throw ProtocolError($"Unexpected host response message:", message);
                }
            } finally {
                await _callbacks.Disconnected();
            }
        }

        private Task Run(CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (_runTask != null) {
                throw new InvalidOperationException("This host is already running.");
            }

            return _runTask = RunWorker(ct);
        }

        public async Task CreateAndRun(string rHome, ProcessStartInfo psi = null, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            string rhostExe = Path.Combine(Path.GetDirectoryName(typeof(RHost).Assembly.ManifestModule.FullyQualifiedName), RHostExe);
            string rBinPath = Path.Combine(rHome, RBinPathX64);

            if (!File.Exists(rhostExe)) {
                throw new MicrosoftRHostMissingException();
            }

            psi = psi ?? new ProcessStartInfo();
            psi.FileName = rhostExe;
            psi.UseShellExecute = false;
            psi.EnvironmentVariables["R_HOME"] = rHome;
            psi.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH") + ";" + rBinPath;

            using (this)
            using (_process = Process.Start(psi)) {
                _log.RHostProcessStarted(psi);
                _process.EnableRaisingEvents = true;
                _process.Exited += delegate { Dispose(); };

                try {
                    ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;
                    using (var socket = new ClientWebSocket()) {
                        var uri = new Uri("ws://localhost:" + DefaultPort);
                        for (int i = 0; ; ++i) {
                            try {
                                await socket.ConnectAsync(uri, ct);
                                _socket = socket;
                                _log.ConnectedToRHostWebSocket(uri, i);
                                break;
                            } catch (WebSocketException) {
                                if (i > 10) {
                                    _log.FailedToConnectToRHost(uri);
                                    throw;
                                }
                                await Task.Delay(100, ct);
                            }
                        }

                        await Run(ct);
                    }
                } catch (OperationCanceledException) when (ct.IsCancellationRequested) {
                    // Expected cancellation, do not propagate, just exit process
                } catch (WebSocketException ex) when (ct.IsCancellationRequested) {
                    // Network errors during cancellation are expected, but should not be exposed to clients.
                    throw new OperationCanceledException(new OperationCanceledException().Message, ex);
                } catch (Exception ex) {
                    Trace.Fail("Exception in RHost run loop:\n" + ex);
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

        public async Task AttachAndRun(Uri uri, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;
            using (var socket = new ClientWebSocket()) {
                await socket.ConnectAsync(uri, ct);
                _socket = socket;
                await Run(ct);
            }
        }
    }
}
