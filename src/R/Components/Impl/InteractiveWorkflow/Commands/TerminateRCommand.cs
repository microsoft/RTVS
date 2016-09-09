// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class TerminateRCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IRSession _session;
        private readonly ICoreShell _coreShell;

        public TerminateRCommand(IRInteractiveWorkflow interactiveWorkflow, ICoreShell coreShell) {
            _interactiveWorkflow = interactiveWorkflow;
            _session = interactiveWorkflow.RSession;
            _coreShell = coreShell;
        }

        public CommandStatus Status {
            get {
                var status = CommandStatus.Supported;
                if (_interactiveWorkflow.ActiveWindow == null) {
                    status |= CommandStatus.Invisible;
                } else if (_session.IsHostRunning) {
                    status |= CommandStatus.Enabled;
                }
                return status;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            if (_coreShell.ShowMessage(Resources.Warning_TerminateR, MessageButtons.YesNo) == MessageButtons.Yes) {
                foreach (var s in _interactiveWorkflow.RSessions.GetSessions().ToList()) {
                    await s.StopHostAsync();
                }
            }
            return CommandResult.Executed;
        }
    }
}
