// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    [Export(typeof(IConnectionManagerProvider))]
    internal class ConnectionManagerProvider : IConnectionManagerProvider {
        private readonly IStatusBar _statusBar;
        private readonly IRSettings _settings;

        [ImportingConstructor]
        public ConnectionManagerProvider(IStatusBar statusBar, IRSettings settings) {
            _statusBar = statusBar;
            _settings = settings;
        }

        public IConnectionManager CreateConnectionManager(IRInteractiveWorkflow interactiveWorkflow) {
            return new ConnectionManager(_statusBar, _settings, interactiveWorkflow);
        }
    }
}