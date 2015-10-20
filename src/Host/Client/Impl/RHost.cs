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

namespace Microsoft.R.Host.Client {
    public sealed class RHost : IDisposable, IRExpressionEvaluator {
        public const int DefaultPort = 5118;
        public const string RHostExe = "Microsoft.R.Host.exe";
        public const string RBinPathX64 = @"bin\x64";

        public static IRContext TopLevelContext { get; } = new RContext(RContextType.TopLevel);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IRCallbacks _callbacks;
        private readonly LinesLog _log;
        private Process _process;
        private ClientWebSocket _socket;
        private byte[] _buffer;
        private bool _isRunning, _canEval;
        private int _rLoopDepth;
        private ulong _messageId;

        public RHost(IRCallbacks callbacks) {
            _callbacks = callbacks;
            _log = new LinesLog(FileLogWriter.InTempFolder("Microsoft.R.Host.Client"));
        }

        public void Dispose() {
            _cts.Cancel();
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
                    using (_socket = new ClientWebSocket()) {
                        var uri = new Uri("ws://localhost:" + DefaultPort);
                        for (int i = 0; ; ++i) {
                            try {
                                await _socket.ConnectAsync(uri, ct);
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
                } catch (OperationCanceledException) when(ct.IsCancellationRequested) {
                    // Expected cancellation, do not propagate, just exit process
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
            using (_socket = new ClientWebSocket()) {
                await _socket.ConnectAsync(uri, ct);
                await Run(ct);
            }
        }

        private async Task Run(CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (_isRunning) {
                throw new InvalidOperationException("This host is already running.");
            }

            _buffer = new byte[0x10000];
            _isRunning = true;
            try {
                try {
                    var webSocketReceiveResult = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), ct);
                    string s = Encoding.UTF8.GetString(_buffer, 0, webSocketReceiveResult.Count);
                    var obj = JObject.Parse(s);
                    int protocolVersion = (int)(double)obj["protocol_version"];
                    Debug.Assert(protocolVersion == 1);
                    string rVersion = (string)obj["R_version"];
                    await _callbacks.Connected(rVersion);
                    _log.EnterRLoop(_rLoopDepth);
                    await RunLoop(ct, allowEval: true, depth: 0);
                } finally {
                    _log.ExitRLoop(_rLoopDepth);
                    await _callbacks.Disconnected();
                }
            } finally {
                _isRunning = false;
            }
        }

        private async Task<JObject> RunLoop(CancellationToken ct, bool allowEval, int depth) {
            TaskUtilities.AssertIsOnBackgroundThread();

            while (!ct.IsCancellationRequested) {
                WebSocketReceiveResult webSocketReceiveResult;
                var s = string.Empty;
                do {
                    webSocketReceiveResult = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), ct);
                    if (webSocketReceiveResult.CloseStatus != null) {
                        return null;
                    }
                    s += Encoding.UTF8.GetString(_buffer, 0, webSocketReceiveResult.Count);
                } while (!webSocketReceiveResult.EndOfMessage);

                // TODO: id should be taken from received message when RHost starts providing it
                _log.Response(s, depth, _messageId);
                JObject obj = JObject.Parse(s);
                var contexts = GetContexts(obj);

                var evt = (string)obj["event"];
                switch (evt) {
                    case "YesNoCancel":
                        await YesNoCancel(contexts, obj, allowEval, depth, ct);
                        break;

                    case "ReadConsole":
                        await ReadConsole(contexts, obj, allowEval, depth, ct);
                        break;

                    case "WriteConsoleEx":
                        await
                            _callbacks.WriteConsoleEx(contexts, (string)obj["buf"], (OutputType)(double)obj["otype"], ct);
                        break;

                    case "ShowMessage":
                        await _callbacks.ShowMessage(contexts, (string)obj["s"], ct);
                        break;

                    case "Busy":
                        await _callbacks.Busy(contexts, (bool)obj["which"], ct);
                        break;

                    case "CallBack":
                        break;

                    case "eval":
                        return obj;

                    case "PlotXaml":
                        await _callbacks.PlotXaml(contexts, (string)obj["filepath"], ct);
                        // TODO: delete temporary xaml and bitmap files
                        break;

                    case "exit":
                        return null;

                    default:
                        throw new InvalidDataException("Unknown event type " + evt);
                }
            }

            return null;
        }

        private async Task YesNoCancel(RContext[] contexts, JObject obj, bool allowEval, int depth, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                _canEval = allowEval;
                YesNoCancel input = await _callbacks.YesNoCancel(contexts, (string)obj["s"], _canEval, ct);
                await SendAsync((double)input, depth, ct);
            } finally {
                _canEval = false;
            }
        }

        private async Task ReadConsole(RContext[] contexts, JObject obj, bool allowEval, int depth, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                _canEval = allowEval;

                var prompt = (string)obj["prompt"];
                var buf = (string)obj["buf"];
                var len = (int)(double)obj["len"];
                var addToHistory = (bool)obj["addToHistory"];

                string input = await _callbacks.ReadConsole(contexts, prompt, buf, len, addToHistory, _canEval, ct);
                input = input.Replace("\r\n", "\n");
                await SendAsync(input, depth, ct);
            } finally {
                _canEval = false;
            }
        }

        private async Task SendAsync<T>(T input, int depth, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (ct.IsCancellationRequested) {
                return;
            }

            var request = JsonConvert.SerializeObject(input);
            byte[] responseBytes = Encoding.UTF8.GetBytes(request);
            await _socket.SendAsync(new ArraySegment<byte>(responseBytes, 0, responseBytes.Length), WebSocketMessageType.Text, true, ct);
            _log.Request(request, depth, ++_messageId);
        }

        private static RContext[] GetContexts(JObject obj) {
            JToken contextsArray;
            if (!obj.TryGetValue("contexts", out contextsArray)) {
                return new RContext[0];
            }

            return ((JArray)contextsArray)
                .Cast<JObject>()
                .Select(ctx => new RContext((RContextType)(double)ctx["callflag"]))
                .ToArray();
        }

        async Task<REvaluationResult> IRExpressionEvaluator.EvaluateAsync(string expression, bool reentrant, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (!_canEval) {
                throw new InvalidOperationException("EvaluateAsync can only be called while ReadConsole or YesNoCancel is pending.");
            }

            _canEval = false;
            try {
                await SendAsync(new {
                    command = "eval",
                    expr = expression.Replace("\r\n", "\n")
                }, _rLoopDepth, ct);

                JObject obj;
                try {
                    _rLoopDepth++;
                    _log.EnterRLoop(_rLoopDepth);
                    obj = await RunLoop(ct, reentrant, _rLoopDepth);
                } finally {
                    _log.ExitRLoop(_rLoopDepth);
                    _rLoopDepth--;
                }

                JToken result, error, parseStatus;
                obj.TryGetValue("result", out result);
                obj.TryGetValue("error", out error);
                obj.TryGetValue("ParseStatus", out parseStatus);

                return new REvaluationResult(
                    result != null ? (string)result : null,
                    error != null ? (string)error : null,
                    parseStatus != null ? (RParseStatus)(double)parseStatus : RParseStatus.Null);
            } finally {
                _canEval = true;
            }
        }
    }
}
