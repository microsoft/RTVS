// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class DeleteProfileCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private IInteractiveWindow OutputWriter => _interactiveWorkflow.ActiveWindow.InteractiveWindow;

        public CommandStatus Status {
            get {
                var connectionManager = _interactiveWorkflow.Connections;
                if (connectionManager.IsConnected && connectionManager.ActiveConnection.IsRemote) {
                    return CommandStatus.SupportedAndEnabled;
                } else {
                    return CommandStatus.NotSupported; 
                }
            }
        }

        public DeleteProfileCommand(IRInteractiveWorkflowVisual interactiveWorkflow) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        public async Task InvokeAsync() {
            var host = string.Empty; 
            try {
                host = _interactiveWorkflow.Connections.ActiveConnection.Uri.Host;
                var ui = _interactiveWorkflow.Shell.UI();
                var button = ui.ShowMessage(Resources.DeleteProfile_DeletionWarning.FormatInvariant(host), MessageButtons.YesNo, MessageType.Warning);
                if(button == MessageButtons.Yes) {
                    await _interactiveWorkflow.RSessions.Broker.DeleteProfileAsync();
                    OutputWriter.WriteLine(Resources.DeleteProfile_Success.FormatInvariant(host));
                }
            } catch (RHostDisconnectedException) {
                OutputWriter.WriteLine(Resources.DeleteProfile_Error.FormatInvariant(host));
            } 
        }
    }
}
