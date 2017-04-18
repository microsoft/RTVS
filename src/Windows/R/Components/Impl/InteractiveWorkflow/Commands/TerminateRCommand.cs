// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class TerminateRCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSession _session;
        private readonly IUIService _ui;

        public TerminateRCommand(IRInteractiveWorkflowVisual interactiveWorkflow, IUIService ui) {
            _interactiveWorkflow = interactiveWorkflow;
            _session = interactiveWorkflow.RSession;
            _ui = ui;
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

        public async Task InvokeAsync() {
            if (_ui.ShowMessage(Resources.Warning_TerminateR, MessageButtons.YesNo) == MessageButtons.Yes) {
                foreach (var s in _interactiveWorkflow.RSessions.GetSessions().ToList()) {
                    await s.StopHostAsync();
                }
            }
        }
    }
}
