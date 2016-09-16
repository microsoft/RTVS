// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.ConnectionManager.Commands {
    public class SwitchToConnectionCommand : IAsyncCommandRange {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;
        private ReadOnlyCollection<IConnection> _recentConnections;

        public SwitchToConnectionCommand(IRInteractiveWorkflow workflow) {
            _connectionManager = workflow.Connections;
            _shell = workflow.Shell;
        }

        public CommandStatus GetStatus(int index) {
            _recentConnections = _connectionManager.RecentConnections;
            if (index >= _recentConnections.Count) {
                return CommandStatus.SupportedAndInvisible;
            }

            return _recentConnections[index] == _connectionManager.ActiveConnection 
                ? CommandStatus.SupportedAndEnabled | CommandStatus.Latched
                : CommandStatus.SupportedAndEnabled;
        }

        public string GetText(int index) {
            if (_recentConnections == null) {
                _recentConnections = _connectionManager.RecentConnections;
            }

            return _recentConnections[index].Name;
        }

        public async Task<CommandResult> InvokeAsync(int index) {
            if (_recentConnections == null) {
                _recentConnections = _connectionManager.RecentConnections;
            }

            if (index < _recentConnections.Count) {
                var connection = _recentConnections[index];
                var progressBarMessage = Resources.ConnectionManager_SwitchConnectionProgressBarMessage.FormatInvariant(_connectionManager.ActiveConnection.Name, connection.Name);
                using (var progressBarSession = _shell.ShowProgressBar(progressBarMessage)) {
                    await _connectionManager.ConnectAsync(connection, progressBarSession.UserCancellationToken);
                }
            }
            return CommandResult.Executed;
        }

        public int MaxCount { get; } = 5;
    }
}
