// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class ConnectionManagerProvider : IConnectionManagerProvider {
        private readonly IStatusBar _statusBar;
        private readonly IRSettings _settings;

        public ConnectionManagerProvider(IServiceContainer services) {
            _statusBar = services.GetService<IStatusBar>();
            _settings = services.GetService<IRSettings>();
        }

        public IConnectionManager CreateConnectionManager(IRInteractiveWorkflow interactiveWorkflow)
            => new ConnectionManager(_statusBar, _settings, interactiveWorkflow as IRInteractiveWorkflowVisual);
    }
}