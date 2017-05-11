// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.InfoBar;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.View;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Export(typeof(IRPackageManagerVisualComponentContainerFactory))]
    internal class VsRPackageManagerVisualComponentContainerFactory : ToolWindowPaneFactory<PackageManagerWindowPane>, IRPackageManagerVisualComponentContainerFactory { 
        private readonly ISearchControlProvider _searchControlProvider;
        private readonly IInfoBarProvider _infoBarProvider;

        [ImportingConstructor]
        public VsRPackageManagerVisualComponentContainerFactory(ISearchControlProvider searchControlProvider, IInfoBarProvider infoBarProvider) {
            _searchControlProvider = searchControlProvider;
            _infoBarProvider = infoBarProvider;
        }

        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, int instanceId = 0) {
            return GetOrCreate(instanceId, i => new PackageManagerWindowPane(packageManager, _searchControlProvider, _infoBarProvider, RToolsSettings.Current, VsAppShell.Current));
        }
    }
}