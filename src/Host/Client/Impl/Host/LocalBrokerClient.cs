// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Threading;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class LocalBrokerClient : BrokerClient {
        private const string RHostBrokerExe = "Microsoft.R.Host.Broker.exe";
        private const string InterpreterId = "local";

        private static readonly bool ShowConsole;
        private static readonly NetworkCredential _credentials = new NetworkCredential("RTVS", Guid.NewGuid().ToString());

        private readonly string _rhostDirectory;
        private readonly string _rHome;
        private readonly BinaryAsyncLock _connectLock = new BinaryAsyncLock();

        private Process _brokerProcess;

        static LocalBrokerClient() {
            // Allow "true" and non-zero integer to enable, otherwise disable.
            string rtvsShowConsole = Environment.GetEnvironmentVariable("RTVS_SHOW_CONSOLE");
            if (!bool.TryParse(rtvsShowConsole, out ShowConsole)) {
                int n;
                if (int.TryParse(rtvsShowConsole, out n) && n != 0) {
                    ShowConsole = true;
                }
            }
        }

        public LocalBrokerClient(string name, string rHome, string rhostDirectory = null)
            : base(name, new Uri(rHome), InterpreterId) {

            _rhostDirectory = rhostDirectory ?? Path.GetDirectoryName(typeof(RHost).Assembly.GetAssemblyPath());
            _rHome = rHome;
        }

        public override async Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) {
            await EnsureBrokerStartedAsync();
            return await base.ConnectAsync(name, callbacks, rCommandLineArguments, timeout, cancellationToken);
        }

        private async Task EnsureBrokerStartedAsync() {
            DisposableBag.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            var lockToken = await _connectLock.WaitAsync();
            try {
                if (!lockToken.IsSet) {
                    await ConnectToBrokerWorker(lockToken);
                    lockToken.Set();
                }
            } finally {
                lockToken.Reset();
            }
        }

        private async Task ConnectToBrokerWorker(IBinaryAsyncLockToken lockToken) {
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
                            $" --logging:logHostOutput true" +
                            $" --logging:logPackets true" +
                            $" --server.urls http://127.0.0.1:0" + // :0 means first available ephemeral port
                            $" --startup:name \"{Name}\"" +
                            $" --startup:writeServerUrlsToPipe {pipeName}" +
                            $" --lifetime:parentProcessId {Process.GetCurrentProcess().Id}" +
                            $" --security:secret \"{_credentials.Password}\"" +
                            $" --R:autoDetect false" +
                            $" --R:interpreters:{InterpreterId}:basePath \"{_rHome.TrimTrailingSlash()}\""
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
                        _connectLock.TryReset();
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
                        throw new RHostDisconnectedException("Timed out while waiting for broker process to report its endpoint URI");
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

                    CreateHttpClient(serverUri[0], _credentials);
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
            try {
                _brokerProcess?.Kill();
            } catch (Exception) {
            }

            _brokerProcess?.Dispose();
        }

        protected override void UpdateCredentials() { }

        protected override void OnCredentialsValidated(bool isValid) {
            if (!isValid) {
                // Local broker authentication should never fail - if it does, it's a bug, and we want to surface it right away.
                const string message = "Authentication failed for local broker";
                Trace.Fail(message);
                throw new RHostDisconnectedException(message);
            }
        }
    }
}
