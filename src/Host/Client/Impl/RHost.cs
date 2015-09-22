using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Support.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client
{
    public sealed class RHost : IDisposable
    {
        private class RExpressionEvaluator : IRExpressionEvaluator {
            private readonly WebSocket _ws;
            private readonly byte[] _buffer;
            private readonly CancellationToken _ct;

            public RExpressionEvaluator(WebSocket ws, byte[] buffer, CancellationToken ct) {
                _ws = ws;
                _buffer = buffer;
                _ct = ct;
            }

            public async Task<REvaluationResult> EvaluateAsync(string expression) {
                string request = JsonConvert.SerializeObject(new {
                    command = "eval",
                    expr = expression
                });

                int count = Encoding.UTF8.GetBytes(request, 0, request.Length, _buffer, 0);
                await _ws.SendAsync(new ArraySegment<byte>(_buffer, 0, count), WebSocketMessageType.Text, true, _ct);

                var wsrr = await _ws.ReceiveAsync(new ArraySegment<byte>(_buffer), _ct);
                if (wsrr.CloseStatus != null) {
                    throw new TaskCanceledException();
                }

                string response = Encoding.UTF8.GetString(_buffer, 0, wsrr.Count);
                var obj = JObject.Parse(response);

                JToken result, error, parseStatus;
                obj.TryGetValue("result", out result);
                obj.TryGetValue("error", out error);
                obj.TryGetValue("ParseStatus", out parseStatus);

                return new REvaluationResult(
                    result != null ? (string)result : null,
                    error != null ? (string)error : null,
                    parseStatus != null ? (RParseStatus)(double)parseStatus : RParseStatus.Null);
            }
        }

        public const int DefaultPort = 5118;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IRCallbacks _callbacks;
        private Process _process;

        public RHost(IRCallbacks callbacks)
        {
            _callbacks = callbacks;
        }

        public Process Process => _process;

        public void Dispose()
        {
            _cts.Cancel();
        }

        public async Task CreateAndRun(ProcessStartInfo psi = null, CancellationToken ct = default(CancellationToken))
        {
            string rhostExe = Path.Combine(Path.GetDirectoryName(typeof(RHost).Assembly.ManifestModule.FullyQualifiedName), "Microsoft.R.Host.exe");

            if(!File.Exists(rhostExe))
            {
                throw new MicrosoftRHostMissingException();
            }

            psi = psi ?? new ProcessStartInfo();
            psi.FileName = rhostExe;

            using (_process = Process.Start(psi))
            {
                try
                {
                    ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;
                    using (var ws = new ClientWebSocket())
                    {
                        var uri = new Uri("ws://localhost:" + DefaultPort);
                        for (int i = 0; ; ++i)
                        {
                            try
                            {
                                await ws.ConnectAsync(uri, ct);
                                break;
                            }
                            catch (WebSocketException)
                            {
                                if (i > 10)
                                {
                                    throw;
                                }
                                await Task.Delay(100, ct);
                            }
                        }

                        await Run(ws, ct);
                    }
                }
                finally
                {
                    if (!_process.HasExited)
                    {
                        try
                        {
                            _process.WaitForExit(500);
                            _process.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
        }

        public async Task AttachAndRun(Uri uri, CancellationToken ct = default(CancellationToken))
        {
            ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;
            using (var ws = new ClientWebSocket())
            {
                await ws.ConnectAsync(uri, ct);
                await Run(ws, ct);
            }
        }

        private async Task Run(WebSocket ws, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new byte[0x10000];

            var wsrr = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            string s = Encoding.UTF8.GetString(buffer, 0, wsrr.Count);
            var obj = JObject.Parse(s);
            int protocolVersion = (int)(double)obj["protocol_version"];
            Debug.Assert(protocolVersion == 1);
            string rVersion = (string)obj["R_version"];
            await _callbacks.Connected(rVersion);

            for (bool done = false; !done && !ct.IsCancellationRequested;)
            {
                wsrr = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (wsrr.CloseStatus != null)
                {
                    break;
                }

                s = Encoding.UTF8.GetString(buffer, 0, wsrr.Count);
                obj = JObject.Parse(s);

                var contexts = GetContexts(obj);
                var evaluator = new RExpressionEvaluator(ws, buffer, ct);

                var evt = (string)obj["event"];
                string response = null;

                switch (evt)
                {
                    case "YesNoCancel":
                        {
                            YesNoCancel input = await _callbacks.YesNoCancel(contexts, evaluator, (string)obj["s"]);
                            response = JsonConvert.SerializeObject((double)input);
                            break;
                        }

                    case "ReadConsole":
                        {
                            string input = await _callbacks.ReadConsole(
                                contexts,
                                evaluator,
                                (string)obj["prompt"],
                                (string)obj["buf"],
                                (int)(double)obj["len"],
                                (bool)obj["addToHistory"]);
                            input = input.Replace("\r\n", "\n");
                            response = JsonConvert.SerializeObject(input);
                            break;
                        }

                    case "WriteConsoleEx":
                        await _callbacks.WriteConsoleEx(contexts, evaluator, (string)obj["buf"], (OutputType)(double)obj["otype"]);
                        break;

                    case "ShowMessage":
                        await _callbacks.ShowMessage(contexts, evaluator, (string)obj["s"]);
                        break;

                    case "Busy":
                        await _callbacks.Busy(contexts, evaluator, (bool)obj["which"]);
                        break;

                    case "CallBack":
                        break;

                    case "exit":
                        done = true;
                        break;

                    default:
                        throw new InvalidDataException("Unknown event type " + evt);
                }

                if (response != null)
                {
                    int count = Encoding.UTF8.GetBytes(response, 0, response.Length, buffer, 0);
                    await ws.SendAsync(new ArraySegment<byte>(buffer, 0, count), WebSocketMessageType.Text, true, ct);
                }
            }

            await _callbacks.Disconnected();
        }

        private static RContext[] GetContexts(JObject obj)
        {
            JToken contextsArray;
            if (!obj.TryGetValue("contexts", out contextsArray))
            {
                return new RContext[0];
            }

            return ((JArray) contextsArray)
                .Cast<JObject>()
                .Select(ctx => new RContext((RContextType) (double) ctx["callflag"]))
                .ToArray();
        }
    }
}
