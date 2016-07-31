// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.ConnectionManager.Commands {
    public class SwitchToConnectionCommand : IAsyncCommandRange {
        private readonly IConnectionManager _connnectionManager;
        private readonly ICoreShell _shell;
        private ReadOnlyCollection<IConnection> _recentConnections;

        public SwitchToConnectionCommand(IRInteractiveWorkflow workflow) {
            _connnectionManager = workflow.Connections;
            _shell = workflow.Shell;
        }

        public CommandStatus GetStatus(int index) {
            _recentConnections = _connnectionManager.RecentConnections;
            if (index >= _recentConnections.Count) {
                return CommandStatus.SupportedAndInvisible;
            }

            return _recentConnections[index] == _connnectionManager.ActiveConnection 
                ? CommandStatus.SupportedAndEnabled | CommandStatus.Latched
                : CommandStatus.SupportedAndEnabled;
        }

        public string GetText(int index) {
            if (_recentConnections == null) {
                _recentConnections = _connnectionManager.RecentConnections;
            }

            return _recentConnections[index].Name;
        }

        public async Task<CommandResult> InvokeAsync(int index) {
            if (_recentConnections == null) {
                _recentConnections = _connnectionManager.RecentConnections;
            }

            if (index < _recentConnections.Count) {
                var connection = _recentConnections[index];
                await connection.ConnectAsync();
            }
            return CommandResult.Executed;
        }

        public int MaxCount { get; } = 5;
    }
}
