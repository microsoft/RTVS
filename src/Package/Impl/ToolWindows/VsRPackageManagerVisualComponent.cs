// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InfoBar;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Export(typeof(IRPackageManagerVisualComponentContainerFactory))]
    internal class VsRPackageManagerVisualComponentContainerFactory : ToolWindowPaneFactory<PackageManagerWindowPane>, IRPackageManagerVisualComponentContainerFactory { 
        private readonly ISearchControlProvider _searchControlProvider;

        [ImportingConstructor]
        public VsRPackageManagerVisualComponentContainerFactory(ISearchControlProvider searchControlProvider, ICoreShell coreShell) : base(coreShell.Services) {
            _searchControlProvider = searchControlProvider;
        }

        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, int instanceId = 0) {
            return GetOrCreate(instanceId, i => new PackageManagerWindowPane(packageManager, _searchControlProvider, Services));
        }
    }
}