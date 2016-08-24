// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.ConnectionManager.Commands {
    public class ReconnectCommand : IAsyncCommand {
        private readonly IConnectionManager _connectionsManager;

        public ReconnectCommand(IRInteractiveWorkflow workflow) {
            _connectionsManager = workflow.Connections;
        }

        public CommandStatus Status => _connectionsManager.IsConnected 
            ? CommandStatus.Supported
            : CommandStatus.SupportedAndEnabled;

        public async Task<CommandResult> InvokeAsync() {
            var connection = _connectionsManager.ActiveConnection;
            if (connection != null) {
                await _connectionsManager.ConnectAsync(connection);
            }
            return CommandResult.Executed;
        }
    }
}