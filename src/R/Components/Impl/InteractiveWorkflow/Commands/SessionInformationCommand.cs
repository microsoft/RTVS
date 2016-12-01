// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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

        public SessionInformationCommand(IRInteractiveWorkflow interactiveWorkflow) {
            _interactiveWorkflow = interactiveWorkflow;
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

            var window = _interactiveWorkflow.ActiveWindow?.InteractiveWindow;
            if (window != null) {
                window.WriteErrorLine(Environment.NewLine + Resources.RServices_Information);
                window.WriteErrorLine("\t" + Resources.Version.FormatInvariant(aboutHost.Version));
                window.WriteErrorLine("\t" + Resources.OperatingSystem.FormatInvariant(aboutHost.OS.VersionString));
                window.WriteErrorLine("\t" + Resources.ProcessorCount.FormatInvariant(aboutHost.ProcessorCount));
                window.WriteErrorLine("\t" + Resources.PhysicalMemory.FormatInvariant(aboutHost.TotalPhysicalMemory, aboutHost.FreePhysicalMemory));
                window.WriteErrorLine("\t" + Resources.VirtualMemory.FormatInvariant(aboutHost.TotalVirtualMemory, aboutHost.FreeVirtualMemory));
                window.WriteErrorLine("\t" + Resources.ConnectedUserCount.FormatInvariant(aboutHost.ConnectedUserCount));
                window.WriteErrorLine(string.Empty);
            }

            if (reportTelemetry) {
                var services = _interactiveWorkflow.Shell.Services;
                foreach (var name in aboutHost.Interpreters) {
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote Interpteter", name);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote OS", aboutHost.OS.VersionString);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote CPUs", aboutHost.ProcessorCount);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote RAM", aboutHost.TotalPhysicalMemory);
                }
            }
        }
    }
}
