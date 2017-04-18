// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.View {
    public interface IVisualComponentContainerFactory {
        IVisualComponentContainer<T> GetOrCreate<T>(int instanceId = 0) where T : IVisualComponent;
    }
}