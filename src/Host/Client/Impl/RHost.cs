using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.R.Support.Settings;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client
{
    public sealed class RHost : IDisposable
    {
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
            string rBinPath = RToolsSettings.GetBinariesFolder();
            string rhostExe = Path.Combine(Path.GetDirectoryName(typeof(RHost).Assembly.ManifestModule.FullyQualifiedName), "Microsoft.R.Host.exe");

            if(!File.Exists(rhostExe))
            {
                MessageBox.Show(Resources.Error_Microsoft_R_Host_Missing, "Microsoft Visual Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // TODO: provide actual download link for Microsoft.R.Host.exe
                Process.Start("https://cran.r-project.org");
                return;
            }

            psi = psi ?? new ProcessStartInfo();
            psi.FileName = rhostExe;
            psi.WorkingDirectory = rBinPath;

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

                var evt = (string)obj["event"];
                string response = null;

                switch (evt)
                {
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
                            input = input.Replace("\r\n", "\n");
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
