// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.Components.ConnectionManager.Commands {
    public class SwitchToConnectionCommand : IAsyncCommandRange {
        private readonly IRSettings _settings;
        private readonly IConnectionManager _connectionManager;
        private readonly IUIService _ui;
        private ReadOnlyCollection<IConnection> _recentConnections;

        public SwitchToConnectionCommand(IRInteractiveWorkflow workflow, IRSettings settings) {
            _settings = settings;
            _connectionManager = workflow.Connections;
            _ui = workflow.Shell.UI();
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

        public Task InvokeAsync(int index) {
            if (_recentConnections == null) {
                _recentConnections = _connectionManager.RecentConnections;
            }

            if (index < _recentConnections.Count) {
                var connection = _recentConnections[index];
                var activeConnection = _connectionManager.ActiveConnection;
                if (activeConnection != null && connection.BrokerConnectionInfo == activeConnection.BrokerConnectionInfo) {
                    var text = Resources.ConnectionManager_ConnectionsAreIdentical.FormatCurrent(activeConnection.Name, connection.Name);
                    _ui.ShowMessage(text, MessageButtons.OK);
                } else {
                    if (activeConnection != null && _settings.ShowWorkspaceSwitchConfirmationDialog == YesNo.Yes) {
                        var message = Resources.ConnectionManager_SwitchConfirmation.FormatCurrent(activeConnection.Name, connection.Name);
                        if (_ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.No) {
                            return Task.CompletedTask;
                        }
                    }

                    var progressBarMessage = activeConnection != null
                        ? Resources.ConnectionManager_SwitchConnectionProgressBarMessage.FormatCurrent(activeConnection.Name, connection.Name)
                        : Resources.ConnectionManager_ConnectionToProgressBarMessage.FormatCurrent(connection.Name);
                    _ui.ProgressDialog.Show(ct => _connectionManager.ConnectAsync(connection, ct), progressBarMessage);
                }
            }

            return Task.CompletedTask;
        }

        public int MaxCount { get; } = 5;
    }
}
