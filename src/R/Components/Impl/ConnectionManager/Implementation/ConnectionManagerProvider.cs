// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    [Export(typeof(IConnectionManagerProvider))]
    internal class ConnectionManagerProvider : IConnectionManagerProvider {
        private readonly IStatusBar _statusBar;
        private readonly IRSettings _settings;
        private readonly IActionLog _log;

        [ImportingConstructor]
        public ConnectionManagerProvider(IStatusBar statusBar, IRSettings settings, IActionLog log) {
            _statusBar = statusBar;
            _settings = settings;
            _log = log;
        }

        public IConnectionManager CreateConnectionManager(IRInteractiveWorkflow interactiveWorkflow) {
            return new ConnectionManager(_statusBar, _settings, interactiveWorkflow, _log);
        }
    }
}