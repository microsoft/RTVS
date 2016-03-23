// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.View;

namespace Microsoft.R.Components.PackageManager {
    public interface IRPackageManagerVisualComponentContainerFactory {
        IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, int instanceId = 0);
    }
}
