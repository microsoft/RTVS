// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.UI.Commands;
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

            _interactiveWorkflow.RSessions.BrokerChanged += OnBrokerChanged;
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

        public Task InvokeAsync() => PrintBrokerInformationAsync();

        private void OnBrokerChanged(object sender, EventArgs e) {
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

            var sb = new StringBuilder();

            sb.AppendLine(Environment.NewLine + Resources.RServices_Information);
            var broker = _interactiveWorkflow.RSessions.Broker;
            if (broker.IsRemote) {
                sb.AppendLine("\t" + Resources.RemoteConnection.FormatInvariant(broker.Name, broker.ConnectionInfo.Uri));
            } else {
                sb.AppendLine("\t" + Resources.LocalR.FormatInvariant(broker.ConnectionInfo.Uri.LocalPath));
            }

            sb.AppendLine("\t" + Resources.Version.FormatInvariant(aboutHost.Version));
            sb.AppendLine("\t" + Resources.OperatingSystem.FormatInvariant(aboutHost.OS.VersionString));
            sb.AppendLine("\t" + Resources.ProcessorCount.FormatInvariant(aboutHost.ProcessorCount));
            sb.AppendLine("\t" + Resources.PhysicalMemory.FormatInvariant(aboutHost.TotalPhysicalMemory, aboutHost.FreePhysicalMemory));
            sb.AppendLine("\t" + Resources.VirtualMemory.FormatInvariant(aboutHost.TotalVirtualMemory, aboutHost.FreeVirtualMemory));

            if (!string.IsNullOrEmpty(aboutHost.VideoCardName)) {
                sb.AppendLine("\t" + Resources.VideoCardName.FormatInvariant(aboutHost.VideoCardName));
            }

            if (!string.IsNullOrEmpty(aboutHost.VideoProcessor)) {
                sb.AppendLine("\t" + Resources.VideoProcessor.FormatInvariant(aboutHost.VideoProcessor));
            }

            if (aboutHost.VideoRAM > 0) {
                sb.AppendLine("\t" + Resources.VideoRAM.FormatInvariant(aboutHost.VideoRAM));
            }

            sb.AppendLine("\t" + Resources.ConnectedUserCount.FormatInvariant(aboutHost.ConnectedUserCount));
            sb.AppendLine(string.Empty);

            if (_interactiveWorkflow.RSession.IsRemote) {
                sb.AppendLine(Resources.InstalledInterpreters);

                int count = 0;
                foreach (var interpreter in aboutHost.Interpreters) {
                    sb.AppendLine("\t" + interpreter);
                    count++;
                }

                sb.AppendLine(string.Empty);
                if (count > 1) {
                    sb.AppendLine(Resources.SelectInterpreterInstruction + Environment.NewLine);
                }
            }

            _console.WriteError(sb.ToString());
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
