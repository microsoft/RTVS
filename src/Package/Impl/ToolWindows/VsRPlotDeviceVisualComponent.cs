// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Export(typeof(IRPlotDeviceVisualComponentContainerFactory))]
    internal class VsRPlotDeviceVisualComponentContainerFactory : ToolWindowPaneFactory<PlotDeviceWindowPane>, IRPlotDeviceVisualComponentContainerFactory {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public VsRPlotDeviceVisualComponentContainerFactory(ICoreShell coreShell) : base(coreShell.Services) {
            _coreShell = coreShell;
        }

        public IVisualComponentContainer<IRPlotDeviceVisualComponent> GetOrCreate(IRPlotManagerVisual plotManager, IRSession session, int instanceId = 0) {
            return GetOrCreate(instanceId, i => new PlotDeviceWindowPane(plotManager, session, i, _coreShell.Services));
        }
    }
}
