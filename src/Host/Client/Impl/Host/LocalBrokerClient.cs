// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Client.Host {
    public sealed class LocalBrokerClient : BrokerClient {
        private const string RHostBrokerExe = "Microsoft.R.Host.Broker.Windows.exe";
        private const string RHostExe = "Microsoft.R.Host.exe";
        private const string InterpreterId = "local";

        private static readonly bool ShowConsole;
        private static readonly LocalCredentialsDecorator _credentials = new LocalCredentialsDecorator();

        private readonly string _rhostDirectory;
        private readonly string _rHome;
        private readonly BinaryAsyncLock _connectLock = new BinaryAsyncLock();
        private readonly IServiceContainer _services;

        private Process _brokerProcess;

        static LocalBrokerClient() {
            // Allow "true" and non-zero integer to enable, otherwise disable.
            var rtvsShowConsole = Environment.GetEnvironmentVariable("RTVS_SHOW_CONSOLE");
            if (!bool.TryParse(rtvsShowConsole, out ShowConsole)) {
                int n;
                if (int.TryParse(rtvsShowConsole, out n) && n != 0) {
                    ShowConsole = true;
                }
            }
        }

        public LocalBrokerClient(string name, BrokerConnectionInfo connectionInfo, IServiceContainer services, IConsole console, string rhostDirectory = null)
            : base(name, connectionInfo, _credentials, console, services) {
            _rHome = connectionInfo.Uri.LocalPath;
            _services = services;
            _rhostDirectory = rhostDirectory ?? Path.GetDirectoryName(typeof(RHost).GetTypeInfo().Assembly.GetAssemblyPath());

            IsVerified = true;
        }

        public override async Task<RHost> ConnectAsync(HostConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) {
            await EnsureBrokerStartedAsync(cancellationToken);
            return await base.ConnectAsync(connectionInfo, cancellationToken);
        }

        private async Task EnsureBrokerStartedAsync(CancellationToken cancellationToken) {
            DisposableBag.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            var lockToken = await _connectLock.WaitAsync(cancellationToken);
            try {
                if (!lockToken.IsSet) {
                    await ConnectToBrokerWorker(cancellationToken);
                }
                lockToken.Set();
            } finally {
                lockToken.Reset();
            }
        }

        private async Task ConnectToBrokerWorker(CancellationToken cancellationToken) {
            Trace.Assert(_brokerProcess == null);

            var rhostExe = Path.Combine(_rhostDirectory, RHostExe);
            if (!_services.FileSystem().FileExists(rhostExe)) {
                throw new RHostBinaryMissingException();
            }

            var rhostBrokerExe = Path.Combine(_rhostDirectory, RHostBrokerExe);
            if (!_services.FileSystem().FileExists(rhostBrokerExe)) {
                throw new RHostBrokerBinaryMissingException();
            }

            Process process = null;
            try {
                var pipeName = Guid.NewGuid().ToString();
                var cts = new CancellationTokenSource(100000);

                using (var processConnectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                using (var serverUriPipe = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)) {
                    var psi = new ProcessStartInfo {
                        FileName = rhostBrokerExe,
                        UseShellExecute = false,
                        Arguments =
                            $" --logging:logFolder \"{Log.Folder.TrimTrailingSlash()}\"" +
                            $" --logging:logHostOutput {Log.LogVerbosity >= LogVerbosity.Normal}" +
                            $" --logging:logPackets {Log.LogVerbosity == LogVerbosity.Traffic}" +
                            $" --urls http://127.0.0.1:0" + // :0 means first available ephemeral port
                            $" --startup:name \"{Name}\"" +
                            $" --startup:writeServerUrlsToPipe {pipeName}" +
                            $" --lifetime:parentProcessId {Process.GetCurrentProcess().Id}" +
                            $" --security:secret \"{_credentials.Password}\"" +
                            $" --R:autoDetect false" +
                            $" --R:interpreters:{InterpreterId}:name \"{Name}\"" +
                            $" --R:interpreters:{InterpreterId}:basePath \"{_rHome.TrimTrailingSlash()}\""
                    };

                    if (!ShowConsole) {
                        psi.CreateNoWindow = true;
                    }

                    process = StartBroker(psi);
                    process.EnableRaisingEvents = true;

                    process.Exited += delegate {
                        cts.Cancel();
                        _brokerProcess = null;
                        _connectLock.EnqueueReset();
                    };

                    await serverUriPipe.WaitForConnectionAsync(processConnectCts.Token);

                    var serverUriData = new MemoryStream();
                    try {
                        // Pipes are special in that a zero-length read is not an indicator of end-of-stream.
                        // Stream.CopyTo uses a zero-length read as indicator of end-of-stream, so it cannot 
                        // be used here. Instead, copy the data manually, using PipeStream.IsConnected to detect
                        // when the other side has finished writing and closed the pipe.
                        var buffer = new byte[0x1000];
                        do {
                            var count = await serverUriPipe.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                            serverUriData.Write(buffer, 0, count);
                        } while (serverUriPipe.IsConnected);
                    } catch (OperationCanceledException) {
                        throw new RHostDisconnectedException("Timed out while waiting for broker process to report its endpoint URI");
                    }

                    var serverUriStr = Encoding.UTF8.GetString(serverUriData.ToArray());
                    Uri[] serverUri;
                    try {
                        serverUri = Json.DeserializeObject<Uri[]>(serverUriStr);
                    } catch (JsonSerializationException ex) {
                        throw new RHostDisconnectedException($"Invalid JSON for endpoint URIs received from broker ({ex.Message}): {serverUriStr}");
                    }
                    if (serverUri?.Length != 1) {
                        throw new RHostDisconnectedException($"Unexpected number of endpoint URIs received from broker: {serverUriStr}");
                    }

                    CreateHttpClient(serverUri[0]);
                }

                if (DisposableBag.TryAdd(DisposeBrokerProcess)) {
                    _brokerProcess = process;
                }
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

        private Process StartBroker(ProcessStartInfo psi) {
            var process = _services.Process().Start(psi);
            process.WaitForExit(250);
            if (process.HasExited && process.ExitCode < 0) {
                var message = _services.Process().MessageFromExitCode(process.ExitCode);
                if (!string.IsNullOrEmpty(message)) {
                    throw new RHostDisconnectedException(Resources.Error_UnableToStartBrokerException.FormatInvariant(Name, message), new Win32Exception(message));
                }
                throw new RHostDisconnectedException(Resources.Error_UnableToStartBrokerException.FormatInvariant(Name, process.ExitCode), new Win32Exception(process.ExitCode));
            }
            return process;
        }

        private void DisposeBrokerProcess() {
            try {
                _brokerProcess?.Kill();
            } catch (Exception) {
            }

            _brokerProcess?.Dispose();
        }
    }
}
