// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Components.Controller;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class SessionInformationCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public SessionInformationCommand(IRInteractiveWorkflow interactiveWorkflow) {
            _interactiveWorkflow = interactiveWorkflow;
            _interactiveWorkflow.RSessions.BrokerChanged += OnBrokerChanged;
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

        private void OnBrokerChanged(object sender, EventArgs e) {
            if (_interactiveWorkflow.RSession.IsRemote) {
                PrintBrokerInformationAsync(reportTelemetry: true).DoNotWait();
            }
        }

        private async Task PrintBrokerInformationAsync(bool reportTelemetry = false) {
            var a = await _interactiveWorkflow.RSessions.Broker.GetHostInformationAsync();
            var window = _interactiveWorkflow.ActiveWindow?.InteractiveWindow;

            if (window != null) {
                window.WriteErrorLine(Environment.NewLine + Resources.RServices_Information);
                window.WriteErrorLine("\t" + Resources.Version.FormatInvariant(a.Version));
                window.WriteErrorLine("\t" + Resources.OperatingSystem.FormatInvariant(a.OS.VersionString));
                window.WriteErrorLine("\t" + Resources.ProcessorCount.FormatInvariant(a.ProcessorCount));
                window.WriteErrorLine("\t" + Resources.PhysicalMemory.FormatInvariant(a.TotalPhysicalMemory, a.FreePhysicalMemory));
                window.WriteErrorLine("\t" + Resources.VirtualMemory.FormatInvariant(a.TotalVirtualMemory, a.FreeVirtualMemory));
                window.WriteErrorLine("\t" + Resources.ConnectedUserCount.FormatInvariant(a.ConnectedUserCount));
                window.WriteErrorLine(string.Empty);
            }

            if (reportTelemetry) {
                var services = _interactiveWorkflow.Shell.Services;
                foreach (var name in a.Interpreters) {
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote Interpteter", name);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote OS", a.OS.VersionString);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote CPUs", a.ProcessorCount);
                    services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote RAM", a.TotalPhysicalMemory);
                }
            }
        }
    }
}
