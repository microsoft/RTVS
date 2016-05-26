// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.Plots {
    [Export(typeof(IRPlotManagerVisualComponentContainerFactory))]
    class VsRPlotManagerVisualComponentContainerFactory : ToolWindowPaneFactory<PlotManagerWindowPane>, IRPlotManagerVisualComponentContainerFactory {
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        [ImportingConstructor]
        public VsRPlotManagerVisualComponentContainerFactory(IRInteractiveWorkflowProvider workflowProvider, IInteractiveWindowComponentContainerFactory componentContainerFactory) {
            _workflowProvider = workflowProvider;
            _componentContainerFactory = componentContainerFactory;
        }

        public IVisualComponentContainer<IRPlotManagerVisualComponent> GetOrCreate(IRPlotManager plotManager, IRSession session, int instanceId = 0) {
            var workflow = _workflowProvider.GetOrCreate();
            if (workflow.ActiveWindow == null) {
                VsAppShell.Current.DispatchOnUIThread(() => workflow.GetOrCreateVisualComponent(_componentContainerFactory).DoNotWait());
            }
            return GetOrCreate(instanceId, i => new PlotManagerWindowPane(plotManager, session, RToolsSettings.Current, VsAppShell.Current));
        }
    }
}
