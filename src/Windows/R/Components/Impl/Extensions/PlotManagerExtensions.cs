// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Plots { 
    public static class PlotManagerExtensions {
        public static IRPlotDeviceVisualComponent GetOrCreateVisualComponent(this IRPlotManager pm, IRPlotDeviceVisualComponentContainerFactory factory, int id)
            => ((IRPlotManagerVisual)pm).GetOrCreateVisualComponent(factory, id);

        public static IRPlotHistoryVisualComponent GetOrCreateVisualComponent(this IRPlotManager pm, IRPlotHistoryVisualComponentContainerFactory factory, int id)
            => ((IRPlotManagerVisual)pm).GetOrCreateVisualComponent(factory, id);
    }
}
