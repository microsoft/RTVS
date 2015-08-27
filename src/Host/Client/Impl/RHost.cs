using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace Microsoft.R.Host {
    public sealed class RHost : IDisposable {
        public const int DefaultPort = 5118;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IRCallbacks _callbacks;
        private Process _process;

        public RHost(IRCallbacks callbacks) {
            _callbacks = callbacks;
        }

        public Process Process {
            get { return _process; }
        }

        public void Dispose() {
            _cts.Cancel();
            _callbacks.Dispose();
        }

        public async Task CreateAndRun(ProcessStartInfo psi = null, CancellationToken ct = default(CancellationToken)) {
            string rPath;
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var r64 = hklm.OpenSubKey(@"Software\R-core\R64")) {
                rPath = r64.GetValue("InstallPath") as string;
                if (rPath == null) {
                    throw new InvalidOperationException("Unable to determine path to R 64-bit");
                }
            }

            string rBinPath = Path.Combine(rPath, @"bin\x64");

            string rhostExe = Path.Combine(
                Path.GetDirectoryName(typeof(RHost).Assembly.ManifestModule.FullyQualifiedName),
                "Microsoft.R.Host.exe");

            psi = psi ?? new ProcessStartInfo();
            psi.FileName = rhostExe;
            psi.WorkingDirectory = rBinPath;

            using (_process = Process.Start(psi)) {
                try {

                    ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;
                    using (var ws = new ClientWebSocket()) {
                        var uri = new Uri("ws://localhost:" + DefaultPort);
                        for (int i = 0; ; ++i) {
                            try {
                                await ws.ConnectAsync(uri, ct);
                                break;
                            } catch (WebSocketException) {
                                if (i > 10) {
                                    throw;
                                }
                                await Task.Delay(100);
                            }
                        }

                        await Run(ws, ct);
                    }
                } finally {
                    if (!_process.HasExited) {
                        try {
                            _process.WaitForExit(500);
                            _process.Kill();
                        } catch (InvalidOperationException) {
                        }
                    }
                }
            }
        }
        public async Task AttachAndRun(Uri uri, CancellationToken ct = default(CancellationToken)) {
            ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;
            using (var ws = new ClientWebSocket()) {
                await ws.ConnectAsync(uri, ct);
                await Run(ws, ct);
            }
        }

        private async Task Run(WebSocket ws, CancellationToken ct = default(CancellationToken)) {
            var buffer = new byte[0x10000];

            var wsrr = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            string s = Encoding.UTF8.GetString(buffer, 0, wsrr.Count);
            var obj = JObject.Parse(s);
            int protocolVersion = (int)(double)obj["protocol_version"];
            Debug.Assert(protocolVersion == 1);
            string rVersion = (string)obj["R_version"];
            await _callbacks.Connected(rVersion);

            for (bool done = false; !done;) {
                wsrr = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (wsrr.CloseStatus != null) {
                    break;
                }

                s = Encoding.UTF8.GetString(buffer, 0, wsrr.Count);
                obj = JObject.Parse(s);

                RContext[] contexts;
                JToken contextsArray;
                if (obj.TryGetValue("contexts", out contextsArray)) {
                    contexts = (from JObject ctx in (JArray)contextsArray
                                select new RContext { CallFlag = (RContextType)(double)ctx["callflag"] }
                               ).ToArray();
                } else {
                    contexts = new RContext[0];
                }

                var evt = (string)obj["event"];
                string response = null;

                switch (evt) {
                    case "YesNoCancel":
                        {
                            YesNoCancel input = await _callbacks.YesNoCancel(contexts, (string)obj["s"]);
                            response = JsonConvert.SerializeObject((double)input);
                            break;
                        }

                    case "ReadConsole":
                        {
                            string input = await _callbacks.ReadConsole(
                                contexts,
                                (string)obj["prompt"],
                                (string)obj["buf"],
                                (int)(double)obj["len"],
                                (bool)obj["addToHistory"]);
                            response = JsonConvert.SerializeObject(input);
                            break;
                        }

                    case "WriteConsoleEx":
                        await _callbacks.WriteConsoleEx(contexts, (string)obj["buf"], (OutputType)(double)obj["otype"]);
                        break;

                    case "ShowMessage":
                        await _callbacks.ShowMessage(contexts, (string)obj["s"]);
                        break;

                    case "Busy":
                        await _callbacks.Busy(contexts, (bool)obj["which"]);
                        break;

                    case "CallBack":
                        break;

                    case "exit":
                        done = true;
                        break;

                    default:
                        throw new InvalidDataException("Unknown event type " + evt);
                }

                if (response != null) {
                    int count = Encoding.UTF8.GetBytes(response, 0, response.Length, buffer, 0);
                    await ws.SendAsync(new ArraySegment<byte>(buffer, 0, count), WebSocketMessageType.Text, true, ct);
                }
            }

            await _callbacks.Disconnected();
        }
    }
}
