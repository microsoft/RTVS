// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Components.Controller;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class SessionInformationCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IConsole _console;

        public SessionInformationCommand(IRInteractiveWorkflow interactiveWorkflow, IConsole console) {
            _interactiveWorkflow = interactiveWorkflow;
            _console = console;

            _interactiveWorkflow.RSession.Interactive += OnRSessionInteractive;
            _interactiveWorkflow.RSession.Disposed += OnRSessionDisposed;
        }

        public CommandStatus Status {
            get {
                var status = CommandStatus.Supported;
                if (_interactiveWorkflow.ActiveWindow == null) {
                    status |= CommandStatus.Invisible;
                } else if (_interactiveWorkflow.RSession.IsHostRunning) {
                    status |= CommandStatus.Enabled;
                }
                return status;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            await PrintBrokerInformationAsync();
            return CommandResult.Executed;
        }

        private void OnRSessionInteractive(object sender, EventArgs e) {
            if (_interactiveWorkflow.RSession.IsRemote) {
                ReplInitComplete().ContinueWith(async (t) => await PrintBrokerInformationAsync(reportTelemetry: true)).DoNotWait();
            }
        }

        private void OnRSessionDisposed(object sender, EventArgs e) {
            _cts.Cancel();
        }

        private Task ReplInitComplete() {
            var iw = _interactiveWorkflow.ActiveWindow?.InteractiveWindow;
            if (iw != null && iw.IsInitializing) {
                return Task.Run(async () => {
                    while (iw.IsInitializing && !_cts.IsCancellationRequested) {
                        await Task.Delay(100);
                    }
                });
            }
            return Task.CompletedTask;
        }

        private async Task PrintBrokerInformationAsync(bool reportTelemetry = false) {
            AboutHost aboutHost;
            try {
                aboutHost = await _interactiveWorkflow.RSessions.Broker.GetHostInformationAsync<AboutHost>();
                if (aboutHost == null) {
                    return;
                }
            } catch (RHostDisconnectedException) {
                return;
            }

            _console.WriteLine(Environment.NewLine + Resources.RServices_Information);
            var broker = _interactiveWorkflow.RSessions.Broker;
            if (broker.IsRemote) {
                _console.WriteLine("\t" + Resources.RemoteConnection.FormatInvariant(broker.Name, broker.ConnectionInfo.Uri));
            } else {
                _console.WriteLine("\t" + Resources.LocalR.FormatInvariant(broker.ConnectionInfo.Uri.LocalPath));
            }

            _console.WriteLine("\t" + Resources.Version.FormatInvariant(aboutHost.Version));
            _console.WriteLine("\t" + Resources.OperatingSystem.FormatInvariant(aboutHost.OS.VersionString));
            _console.WriteLine("\t" + Resources.ProcessorCount.FormatInvariant(aboutHost.ProcessorCount));
            _console.WriteLine("\t" + Resources.PhysicalMemory.FormatInvariant(aboutHost.TotalPhysicalMemory, aboutHost.FreePhysicalMemory));
            _console.WriteLine("\t" + Resources.VirtualMemory.FormatInvariant(aboutHost.TotalVirtualMemory, aboutHost.FreeVirtualMemory));

            if (!string.IsNullOrEmpty(aboutHost.VideoCardName)) {
                _console.WriteLine("\t" + Resources.VideoCardName.FormatInvariant(aboutHost.VideoCardName));
            }

            if (!string.IsNullOrEmpty(aboutHost.VideoProcessor)) {
                _console.WriteLine("\t" + Resources.VideoProcessor.FormatInvariant(aboutHost.VideoProcessor));
            }

            if (aboutHost.VideoRAM > 0) {
                _console.WriteLine("\t" + Resources.VideoRAM.FormatInvariant(aboutHost.VideoRAM));
            }

            _console.WriteLine("\t" + Resources.ConnectedUserCount.FormatInvariant(aboutHost.ConnectedUserCount));
            _console.WriteLine(string.Empty);

            if (_interactiveWorkflow.RSession.IsRemote) {
                _console.WriteLine(Resources.InstalledInterpreters);

                int count = 0;
                foreach (var interpreter in aboutHost.Interpreters) {
                    _console.WriteLine("\t" + interpreter);
                    count++;
                }

                _console.WriteLine(string.Empty);
                if (count > 1) {
                    _console.WriteLine(Resources.SelectInterpreterInstruction);
                }
            }

            var clientVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (clientVersion.Major != 0 || clientVersion.Minor != 0) { // Filter out debug builds
                string message = null;
                if (aboutHost.Version.Major > clientVersion.Major || aboutHost.Version.Minor > clientVersion.Minor) {
                    message = Resources.Warning_RemoteVersionHigher.FormatInvariant(aboutHost.Version, clientVersion);
                } else if (aboutHost.Version.Major < clientVersion.Major || aboutHost.Version.Minor < clientVersion.Minor) {
                    message = Resources.Warning_RemoteVersionLower.FormatInvariant(aboutHost.Version, clientVersion);
                }
                if(!string.IsNullOrEmpty(message)) {
                    _console.WriteLine(Environment.NewLine + message + Environment.NewLine);
                }
            }

            if (reportTelemetry) {
                var services = _interactiveWorkflow.Shell.Services;
                foreach (var name in aboutHost.Interpreters) {
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote Interpteter", name);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote OS", aboutHost.OS.VersionString);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote CPUs", aboutHost.ProcessorCount);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote RAM", aboutHost.TotalPhysicalMemory);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote Video Card", aboutHost.VideoCardName);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote GPU", aboutHost.VideoProcessor);
                }
            }
        }
    }
}
