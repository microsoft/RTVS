// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.View;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Export(typeof(IConnectionManagerVisualComponentContainerFactory))]
    internal class VsConnectionManagerVisualComponentContainerFactory : ToolWindowPaneFactory<ConnectionManagerWindowPane>, IConnectionManagerVisualComponentContainerFactory {
        public IVisualComponentContainer<IConnectionManagerVisualComponent> GetOrCreate(IConnectionManager connectionManager, int instanceId = 0) 
            => GetOrCreate(instanceId, i => new ConnectionManagerWindowPane(connectionManager, RToolsSettings.Current, Vsshell.Current));
    }
}