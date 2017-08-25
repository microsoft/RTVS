// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class InterruptRCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSession _session;
        private readonly IServiceContainer _services;

        public InterruptRCommand(IRInteractiveWorkflowVisual interactiveWorkflow, IServiceContainer services) {
            _interactiveWorkflow = interactiveWorkflow;
            _session = interactiveWorkflow.RSession;
            _services = services;
        }

        public CommandStatus Status {
            get {
                var status = CommandStatus.Supported;
                if (_interactiveWorkflow.ActiveWindow == null) {
                    status |= CommandStatus.Invisible;
                } else if (_session.CanInterrupt(_services)) {
                    status |= CommandStatus.Enabled;
                }
                return status;
            }
        }

        public async Task InvokeAsync() {
            if (_session.CanInterrupt(_services)) {
                _interactiveWorkflow.Operations.ClearPendingInputs();
                await _session.CancelAllAsync();
            }
        }
    }
}
