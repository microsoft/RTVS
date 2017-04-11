// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.PackageManager {
    public interface IRPackageManagerVisual: IRPackageManager {
        IRPackageManagerVisualComponent VisualComponent { get; }

        IRPackageManagerVisualComponent GetOrCreateVisualComponent(IRPackageManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0);
   }
}