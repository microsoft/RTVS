// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Export(typeof(IConnectionManagerVisualProvider))]
    internal class VsConnectionManagerVisualProvider : ToolWindowPaneFactory<ConnectionManagerWindowPane>, IConnectionManagerVisualProvider {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public VsConnectionManagerVisualProvider(ICoreShell coreShell): base(coreShell.Services) {
            _coreShell = coreShell;
        }

        public IConnectionManagerVisual GetOrCreate(IConnectionManager connectionManager, int instanceId = 0) 
            => GetOrCreate(instanceId, i => new ConnectionManagerWindowPane(connectionManager, _coreShell.Services)).Component;
    }
}