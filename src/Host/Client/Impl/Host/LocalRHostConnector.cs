// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Threading;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class LocalRHostConnector : RHostConnector {
        private const int DefaultPort = 5118;
        private const string RHostBrokerExe = "Microsoft.R.Host.Broker.exe";
        private const string RBinPathX64 = @"bin\x64";
        private const string InterpreterId = "local";

        private static readonly bool ShowConsole = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RTVS_HOST_CONSOLE"));
        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif

        private static readonly NetworkCredential _credentials = new NetworkCredential("RTVS", Guid.NewGuid().ToString());

        private readonly string _name;
        private readonly string _rhostDirectory;
        private readonly string _rHome;
        private readonly BinaryAsyncLock _connectLock = new BinaryAsyncLock();

        private Process _brokerProcess;

        public LocalRHostConnector(string name, string rHome, string rhostDirectory = null)
            : base(InterpreterId) {

            _name = name;
            _rhostDirectory = rhostDirectory ?? Path.GetDirectoryName(typeof(RHost).Assembly.GetAssemblyPath());
            _rHome = rHome;
        }

        protected override HttpClientHandler GetHttpClientHandler() {
            return new HttpClientHandler {
                Credentials = _credentials
            };
        }

        protected override void ConfigureWebSocketRequest(HttpWebRequest request) {
            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
            request.Credentials = _credentials;
        }

        protected override async Task ConnectToBrokerAsync() {
            DisposableBag.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            try {
                if (await _connectLock.WaitAsync()) {
                    if (_brokerProcess == null) {
                        CreateHttpClient();
                        await ConnectToBrokerWorker();
                    }
                }
            } finally {
                _connectLock.Release();
            }
        }

        private async Task ConnectToBrokerWorker() {
            Trace.Assert(_brokerProcess == null);

            string rhostBrokerExe = Path.Combine(_rhostDirectory, RHostBrokerExe);
            if (!File.Exists(rhostBrokerExe)) {
                throw new RHostBinaryMissingException();
            }

            Process process = null;
            try {
                string pipeName = Guid.NewGuid().ToString();
                using (var serverUriPipe = new NamedPipeServerStream(pipeName, PipeDirection.In)) {
                    var psi = new ProcessStartInfo {
                        FileName = rhostBrokerExe,
                        UseShellExecute = false,
                        Arguments =
                            $" --server.urls http://127.0.0.1:0" + // :0 means first available ephemeral port
                            $" --startup:name \"{_name}\"" +
                            $" --startup:writeServerUrlsToPipe {pipeName}" +
                            $" --lifetime:parentProcessId {Process.GetCurrentProcess().Id}" +
                            $" --security:secret \"{_credentials.Password}\"" +
                            $" --R:autoDetect false" +
                            $" --R:interpreters:{InterpreterId}:basePath \"{_rHome}\""
                    };

                    if (!ShowConsole) {
                        psi.CreateNoWindow = true;
                    }

                    process = Process.Start(psi);
                    process.EnableRaisingEvents = true;

                    var cts = new CancellationTokenSource(10000);
                    process.Exited += delegate {
                        cts.Cancel();
                        _brokerProcess = null;
                        _connectLock.Reset();
                    };

                    await serverUriPipe.WaitForConnectionAsync(cts.Token);

                    var serverUriData = new MemoryStream();
                    try {
                        // Pipes are special in that a zero-length read is not an indicator of end-of-stream.
                        // Stream.CopyTo uses a zero-length read as indicator of end-of-stream, so it cannot 
                        // be used here. Instead, copy the data manually, using PipeStream.IsConnected to detect
                        // when the other side has finished writing and closed the pipe.
                        var buffer = new byte[0x1000];
                        do {
                            int count = await serverUriPipe.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                            serverUriData.Write(buffer, 0, count);
                        } while (serverUriPipe.IsConnected);
                    } catch (OperationCanceledException) {
                        throw new RHostTimeoutException("Timed out while waiting for broker process to report its endpoint URI");
                    }

                    string serverUriStr = Encoding.UTF8.GetString(serverUriData.ToArray());
                    Uri[] serverUri;
                    try {
                        serverUri = JsonConvert.DeserializeObject<Uri[]>(serverUriStr);
                    } catch (JsonSerializationException ex) {
                        throw new RHostDisconnectedException($"Invalid JSON for endpoint URIs received from broker ({ex.Message}): {serverUriStr}");
                    }
                    if (serverUri?.Length != 1) {
                        throw new RHostDisconnectedException($"Unexpected number of endpoint URIs received from broker: {serverUriStr}");
                    }

                    Broker.BaseAddress = serverUri[0];
                }

                _brokerProcess = process;
                DisposableBag.Add(DisposeBrokerProcess);
            } finally {
                if (_brokerProcess == null) {
                    try {
                        process?.Kill();
                    } catch (Exception) {
                    } finally {
                        process?.Dispose();
                    }
                }
            }
        }

        private void DisposeBrokerProcess() {
            _brokerProcess?.Kill();
            _brokerProcess?.Dispose();
        }
    }
}
