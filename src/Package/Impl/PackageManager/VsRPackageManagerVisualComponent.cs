// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.View;

namespace Microsoft.VisualStudio.R.Package.PackageManager {
    [Export(typeof(IRPackageManagerVisualComponentContainerFactory))]
    internal class VsRPackageManagerVisualComponentContainerFactory : ToolWindowPaneFactory<PackageManagerWindowPane>, IRPackageManagerVisualComponentContainerFactory { 
        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, int instanceId) {
            return GetOrCreate(instanceId, i => new PackageManagerWindowPane(packageManager));
        }
    }
}