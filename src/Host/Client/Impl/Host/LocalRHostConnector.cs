// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using WebSocketSharp;
using WebSocketSharp.Server;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class LocalRHostConnector : IRHostConnector {
        public const int DefaultPort = 5118;
        public const string RHostExe = "Microsoft.R.Host.exe";
        public const string RBinPathX64 = @"bin\x64";

        private static readonly bool ShowConsole = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RTVS_HOST_CONSOLE"));
        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif

        private readonly string _rhostDirectory;
        private readonly string _rHome;
        private readonly LinesLog _log;

        public LocalRHostConnector(string rHome, string rhostDirectory = null) {
            _rhostDirectory = rhostDirectory;
            _rHome = rHome;
            _log = new LinesLog(FileLogWriter.InTempFolder("Microsoft.R.Host.BrokerConnector"));
        }

        public async Task<RHost> ConnectToRHost(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) {
            await TaskUtilities.SwitchToBackgroundThread();

            var rhostDirectory = _rhostDirectory ?? Path.GetDirectoryName(typeof(RHost).Assembly.GetAssemblyPath());
            rCommandLineArguments = rCommandLineArguments ?? string.Empty;

            string rhostExe = Path.Combine(rhostDirectory, RHostExe);
            string rBinPath = Path.Combine(_rHome, RBinPathX64);

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

            var transportTcs = new TaskCompletionSource<IMessageTransport>();
            foreach (var port in ports) {
                cancellationToken.ThrowIfCancellationRequested();

                server = new WebSocketServer(port) {
                    ReuseAddress = false,
                    WaitTime = HeartbeatTimeout,
                };

                server.AddWebSocketService("/", () => CreateWebSocketMessageTransport(transportTcs));

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
            NativeMethods.GetShortPathName(_rHome, shortHome, shortHome.Capacity);
            psi.EnvironmentVariables["R_HOME"] = shortHome.ToString();

            psi.EnvironmentVariables["PATH"] = rBinPath + ";" + Environment.GetEnvironmentVariable("PATH");

            if (name != null) {
                psi.Arguments += " --rhost-name " + name;
            }

            psi.Arguments += Invariant($" --rhost-connect ws://127.0.0.1:{server.Port}");

            if (!ShowConsole) {
                psi.CreateNoWindow = true;
            }

            if (!string.IsNullOrWhiteSpace(rCommandLineArguments)) {
                psi.Arguments += Invariant($" {rCommandLineArguments}");
            }

            var process = Process.Start(psi);
            _log.RHostProcessStarted(psi);
            process.EnableRaisingEvents = true;

            // Timeout increased to allow more time in test and code coverage runs.
            await Task.WhenAny(transportTcs.Task, Task.Delay(timeout, cancellationToken)).Unwrap();
            if (!transportTcs.Task.IsCompleted) {
                _log.FailedToConnectToRHost();
                throw new RHostTimeoutException("Timed out waiting for R host process to connect");
            }

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => {
                if (!process.HasExited) {
                    try {
                        process.WaitForExit(500);
                        if (!process.HasExited) {
                            process.Kill();
                            process.WaitForExit();
                        }
                    } catch (InvalidOperationException) { }
                }
                _log.RHostProcessExited();
            });

            var host = new RHost(name, callbacks, transportTcs.Task.Result, process.Id, cts);
            process.Exited += delegate { host.Dispose(); };
            return host;

            //using (this)
            //using (_) {

            //        
            //    } finally {

            //    }
            //}
        }


        private static WebSocketMessageTransport CreateWebSocketMessageTransport(TaskCompletionSource<IMessageTransport> transportTcs) {
            lock (transportTcs) {
                if (transportTcs.Task.IsCompleted) {
                    throw new MessageTransportException("More than one incoming connection.");
                }

                var transport = new WebSocketMessageTransport();
                transportTcs.SetResult(transport);
                return transport;
            }
        }
    }
}
