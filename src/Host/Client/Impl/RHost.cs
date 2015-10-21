using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        private class Message {
            public string Id;
            public string RequestId;
            public string Name;
            public JToken[] Arguments;

            public override string ToString() {
                var array = new JArray(Id);

                if (RequestId != null) {
                    array.Add(":");
                    array.Add(RequestId);
                }

                array.Add(Arguments);
                return array.ToString();
            }

            public void ExpectArguments(int count) {
                if (Arguments.Length != count) {
                    throw ProtocolError($"{count} arguments expected:", this);
                }
            }

            public JToken GetArgument(int i, JTokenType expectedType) {
                var arg = Arguments[i];
                if (arg.Type != expectedType) {
                    throw ProtocolError($"Argument #{i} must be {expectedType}:", this);
                }
                return arg;
            }

            public JToken GetArgument(int i, JTokenType expectedType1, JTokenType expectedType2) {
                var arg = Arguments[i];
                if (arg.Type != expectedType1 && arg.Type != expectedType2) {
                    throw ProtocolError($"Argument #{i} must be {expectedType1} or {expectedType2}:", this);
                }
                return arg;
            }

            public string GetString(int i, bool allowNull = false) {
                var arg = allowNull ? GetArgument(i, JTokenType.String, JTokenType.Null) : GetArgument(i, JTokenType.String);
                if (arg.Type == JTokenType.Null) {
                    return null;
                }
                return (string)arg;
            }

            public int GetInt32(int i) {
                var arg = GetArgument(i, JTokenType.Integer);
                return (int)arg;
            }

            public bool GetBoolean(int i) {
                var arg = GetArgument(i, JTokenType.Boolean);
                return (bool)arg;
            }

            public TEnum GetEnum<TEnum>(int i, params string[] names) {
                var arg = GetArgument(i, JTokenType.Integer, JTokenType.String);

                if (arg.Type == JTokenType.Integer) {
                    return (TEnum)(object)(int)arg;
                }

                int n = Array.IndexOf(names, (string)arg);
                if (n < 0) {
                    throw ProtocolError($"Argument #{i} must be integer, or one of: {string.Join(", ", names)}:", this);
                }

                return (TEnum)(object)n;
            }
        }

        public static IRContext TopLevelContext { get; } = new RContext(RContextType.TopLevel);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IRCallbacks _callbacks;
        private readonly LinesLog _log;
        private Process _process;
        private ClientWebSocket _socket;
        private byte[] _buffer = new byte[0x100000];
        private bool _isRunning, _canEval;
        private int _rLoopDepth;
        private long _nextMessageId = 1;

        public RHost(IRCallbacks callbacks) {
            _callbacks = callbacks;
            _log = new LinesLog(FileLogWriter.InTempFolder("Microsoft.R.Host.Client"));
        }

        public void Dispose() {
            _cts.Cancel();
        }

        private static Exception ProtocolError(FormattableString fs, object message = null) {
            var s = fs.ToString(CultureInfo.InvariantCulture);
            if (message != null) {
                s += "\n\n" + message;
            }
            Trace.Fail(s);
            return new InvalidDataException(s);
        }

        private async Task<Message> ReceiveMessageAsync(CancellationToken ct) {
            var sb = new StringBuilder();
            WebSocketReceiveResult wsrr;
            do {
                wsrr = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), ct);
                if (wsrr.CloseStatus != null) {
                    return null;
                }
                sb.Append(Encoding.UTF8.GetString(_buffer, 0, wsrr.Count));
            } while (!wsrr.EndOfMessage);

            var json = sb.ToString();
            _log.Response(json, _rLoopDepth);

            var token = JToken.Parse(json);
            if (token == null) {
                return null;
            }

            var message = new Message();

            var body = token as JArray;
            if (body == null) {
                throw ProtocolError($"Message must be an array:", token);
            }
            if (body.Count < 2) {
                throw ProtocolError($"Message must have form [id, name, ...]:", token);
            }

            var id = body[0];
            if (id.Type != JTokenType.String) {
                throw ProtocolError($"Message id must be a string:", token);
            }
            message.Id = (string)id;

            var name = body[1];
            if (name.Type != JTokenType.String) {
                throw ProtocolError($"Message name must be a string:", token);
            }
            message.Name = (string)name;

            int argsOffset = 2;

            if (message.Name == ":") {
                if (body.Count < 4) {
                    throw ProtocolError($"Response message must have form [id, ':', request_id, name, ...]:", token);
                }

                var requestId = body[2];
                if (requestId.Type != JTokenType.String) {
                    throw ProtocolError($"Response message request_id must be a string:", token);
                }
                message.RequestId = (string)requestId;

                name = body[3];
                if (name.Type != JTokenType.String) {
                    throw ProtocolError($"Response message name must be a string:", token);
                }
                message.Name = (string)name;

                argsOffset += 2;
            }

            message.Arguments = body.Skip(argsOffset).ToArray();

            return message;
        }

        private async Task SendAsync(JToken token, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var json = JsonConvert.SerializeObject(token);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, ct);

            _log.Request(json, _rLoopDepth);
        }

        private JArray CreateMessage(CancellationToken ct, out string id, string name, params object[] args) {
            id = "#" + _nextMessageId + "#";
            _nextMessageId += 2;
            return new JArray(id, name, args);
        }

        private async Task<string> SendAsync(string name, CancellationToken ct, params object[] args) {
            string id;
            var message = CreateMessage(ct, out id, name, args);
            await SendAsync(message, ct);
            return id;
        }

        private async Task<string> RespondAsync(Message request, CancellationToken ct, params object[] args) {
            string id;
            var message = CreateMessage(ct, out id, ":", request.Id, request.Name, args);
            await SendAsync(message, ct);
            return id;
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
                } catch (OperationCanceledException) when (ct.IsCancellationRequested) {
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

            _isRunning = true;
            try {
                try {
                    var message = await ReceiveMessageAsync(ct);
                    if (message.Name != "Microsoft.R.Host" || message.RequestId != null) {
                        throw ProtocolError($"Microsoft.R.Host handshake expected:", message);
                    }

                    var protocolVersion = message.GetInt32(0);
                    if (protocolVersion != 1) {
                        throw ProtocolError($"Unsupported RHost protocol version:", message);
                    }

                    var rVersion = message.GetString(1);
                    await _callbacks.Connected(rVersion);

                    message = await RunLoop(ct, allowEval: true);
                    if (message != null) {
                        throw ProtocolError($"Unexpected host response message:", message);
                    }
                } finally {
                    await _callbacks.Disconnected();
                }
            } finally {
                _isRunning = false;
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

                    switch (message.Name) {
                        case "?":
                            await YesNoCancel(message, allowEval, ct);
                            break;

                        case ">":
                            await ReadConsole(message, allowEval, ct);
                            break;

                        case "!":
                        case "!!":
                            message.ExpectArguments(1);
                            await _callbacks.WriteConsoleEx(
                                message.GetString(0, allowNull: true),
                                message.Name.Length == 1 ? OutputType.Output : OutputType.Error,
                                ct);
                            break;

                        case "![]":
                            message.ExpectArguments(1);
                            await _callbacks.ShowMessage(message.GetString(0, allowNull: true), ct);
                            break;

                        case "~+":
                            await _callbacks.Busy(true, ct);
                            break;
                        case "~-":
                            await _callbacks.Busy(false, ct);
                            break;

                        case "PlotXaml":
                            await _callbacks.PlotXaml(message.GetString(0), ct);
                            // TODO: delete temporary xaml and bitmap files
                            break;

                        default:
                            throw ProtocolError($"Unrecognized host message name:", message);
                    }
                }
            } finally {
                _log.ExitRLoop(--_rLoopDepth);
            }

            return null;
        }

        private static RContext[] GetContexts(Message message) {
            var contexts = message.GetArgument(0, JTokenType.Array)
                .Select((token, i) => {
                    if (token.Type != JTokenType.Integer) {
                        throw ProtocolError($"Element #{i} of context array must be an integer:", message);
                    }
                    return new RContext((RContextType)(int)token);
                });
            return contexts.ToArray();
        }

        private async Task YesNoCancel(Message request, bool allowEval, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(2);
            var contexts = GetContexts(request);
            var s = request.GetString(1, allowNull: true);

            YesNoCancel input;
            try {
                _canEval = allowEval;
                input = await _callbacks.YesNoCancel(contexts, s, _canEval, ct);
            } finally {
                _canEval = false;
            }

            string response;
            switch (input) {
                case Client.YesNoCancel.No:
                    response = "M";
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
                        Trace.Fail(error.ToString(CultureInfo.InvariantCulture));
                        throw new InvalidOperationException(error.ToString(CultureInfo.InvariantCulture));
                    }
            }

            await RespondAsync(request, ct, response);
        }

        private async Task ReadConsole(Message request, bool allowEval, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            request.ExpectArguments(5);

            var contexts = GetContexts(request);
            var len = request.GetInt32(1);
            var addToHistory = request.GetBoolean(2);
            var retryReason = request.GetString(3, allowNull: true);
            var prompt = request.GetString(4, allowNull: true);

            string input;
            try {
                _canEval = allowEval;
                input = await _callbacks.ReadConsole(contexts, prompt, len, addToHistory, _canEval, ct);
            } finally {
                _canEval = false;
            }

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

                response.ExpectArguments(3);
                var parseStatus = response.GetEnum<RParseStatus>(0, "NULL", "OK", "INCOMPLETE", "ERROR", "EOF");
                var error = response.GetString(1, allowNull: true);

                if (jsonResult) {
                    return new REvaluationResult(response.Arguments[2], error, parseStatus);
                } else {
                    return new REvaluationResult(response.GetString(2, allowNull: true), error, parseStatus);
                }
            } finally {
                _canEval = true;
            }
        }
    }
}
