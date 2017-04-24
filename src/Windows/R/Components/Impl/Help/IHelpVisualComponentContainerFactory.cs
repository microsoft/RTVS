// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Help {
    public interface IHelpVisualComponentContainerFactory {
        IVisualComponentContainer<IHelpVisualComponent> GetOrCreate(int instanceId = 0);
    }
}