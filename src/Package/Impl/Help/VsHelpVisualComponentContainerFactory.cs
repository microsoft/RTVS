// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Export(typeof(IHelpVisualComponentContainerFactory))]
    internal class VsHelpVisualComponentContainerFactory : ToolWindowPaneFactory<HelpWindowPane>, IHelpVisualComponentContainerFactory {
        public IVisualComponentContainer<IHelpVisualComponent> GetOrCreate(int instanceId) {
            return GetOrCreate(instanceId, i => new HelpWindowPane(VsAppShell.Current));
        }
    }
}
