// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.PackageManager {
    [Export(typeof(IRPackageManagerVisualComponentContainerFactory))]
    internal class VsRPackageManagerVisualComponentContainerFactory : ToolWindowPaneFactory<PackageManagerWindowPane>, IRPackageManagerVisualComponentContainerFactory { 
        private readonly ISearchControlProvider _searchControlProvider;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        [ImportingConstructor]
        public VsRPackageManagerVisualComponentContainerFactory(ISearchControlProvider searchControlProvider, IRInteractiveWorkflowProvider workflowProvider, IInteractiveWindowComponentContainerFactory componentContainerFactory) {
            _searchControlProvider = searchControlProvider;
            _workflowProvider = workflowProvider;
            _componentContainerFactory = componentContainerFactory;
        }

        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, IRSession session, int instanceId = 0) {
            var workflow = _workflowProvider.GetOrCreate();
            if (workflow.ActiveWindow == null) {
                VsAppShell.Current.DispatchOnUIThread(() => workflow.GetOrCreateVisualComponent(_componentContainerFactory).DoNotWait());
            }
            return GetOrCreate(instanceId, i => new PackageManagerWindowPane(packageManager, session, _searchControlProvider, RToolsSettings.Current, VsAppShell.Current));
        }
    }
}