// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.PackageManager {
    public static class PackageManagerExtensions {
        public static IRPackageManagerVisualComponent GetOrCreateVisualComponent(this IRPackageManager pm, IRPackageManagerVisualComponentContainerFactory factory, int id = 0)
            => ((IRPackageManagerVisual)pm).GetOrCreateVisualComponent(factory, id);
    }
}
