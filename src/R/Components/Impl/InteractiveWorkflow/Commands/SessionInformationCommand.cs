// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class SessionInformationCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public SessionInformationCommand(IRInteractiveWorkflow interactiveWorkflow) {
            _interactiveWorkflow = interactiveWorkflow;
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

        public Task<CommandResult> InvokeAsync() {
            _interactiveWorkflow.RSessions.DisplayBrokerInformation();
            return Task.FromResult(CommandResult.Executed);
        }
    }
}
