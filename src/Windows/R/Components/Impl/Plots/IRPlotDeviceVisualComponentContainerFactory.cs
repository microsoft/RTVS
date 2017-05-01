// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotDeviceVisualComponentContainerFactory {
        IVisualComponentContainer<IRPlotDeviceVisualComponent> GetOrCreate(IRPlotManagerVisual plotManager, IRSession session, int instanceId = 0);
    }
}
