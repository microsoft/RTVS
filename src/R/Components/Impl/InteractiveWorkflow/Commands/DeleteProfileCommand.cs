using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.InteractiveWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class DeleteProfileCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private IInteractiveWindow OutputWriter => _interactiveWorkflow.ActiveWindow.InteractiveWindow;

        public CommandStatus Status {
            get {
                if (_interactiveWorkflow.Connections.IsConnected && _interactiveWorkflow.Connections.ActiveConnection.IsRemote) {
                    return CommandStatus.SupportedAndEnabled;
                } else {
                    return CommandStatus.NotSupported; 
                }
            }
        }

        public DeleteProfileCommand(IRInteractiveWorkflow interactiveWorkflow) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        public async Task<CommandResult> InvokeAsync() {
            await DeleteProfileWorkerAsync();
            return CommandResult.Executed;
        }

        private async Task DeleteProfileWorkerAsync() {
            try {
                var host = _interactiveWorkflow.Connections.ActiveConnection.Uri.Host;
                var button = _interactiveWorkflow.Shell.ShowMessage(Resources.DeleteProfile_DeletionWarning.FormatInvariant(host), MessageButtons.YesNo, MessageType.Warning);
                if(button == MessageButtons.Yes) {
                    bool result = await _interactiveWorkflow.RSessions.Broker.DeleteRemoteUserProfileAsync();
                    if (result) {
                        OutputWriter.WriteLine(Resources.DeleteProfile_Success.FormatInvariant(host));
                    } else {
                        OutputWriter.WriteLine(Resources.DeleteProfile_Error.FormatInvariant(host));
                    }
                }
            } catch (RHostDisconnectedException) {
                return;
            } 
        }
    }
}
