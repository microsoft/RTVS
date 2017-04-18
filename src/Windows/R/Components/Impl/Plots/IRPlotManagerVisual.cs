// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Plots {
    public interface IRPlotManagerVisual : IRPlotManager {
        IRPlotDeviceVisualComponent GetOrCreateVisualComponent(IRPlotDeviceVisualComponentContainerFactory visualComponentContainerFactory, int instanceId);
        IRPlotHistoryVisualComponent GetOrCreateVisualComponent(IRPlotHistoryVisualComponentContainerFactory visualComponentContainerFactory, int instanceId);

        /// <summary>
        /// Visual component for the global plot history, or <c>null</c> if it
        /// hasn't been created yet.
        /// </summary>
        IRPlotHistoryVisualComponent HistoryVisualComponent { get; }

        /// <summary>
        /// Visual component for the plot device, or <c>null</c> if it hasn't
        /// been created yet.
        /// </summary>
        IRPlotDeviceVisualComponent GetPlotVisualComponent(IRPlotDevice device);

        /// <summary>
        /// Visual component for the plot device which uses the specified
        /// instance id, or <c>null</c> if it hasn't been created yet.
        /// </summary>
        IRPlotDeviceVisualComponent GetPlotVisualComponent(int instanceId);

        /// <summary>
        /// The visual component for the active plot device, or the visual component
        /// expected to be used for the next plot operation. A visual component
        /// will be created if necessary.
        /// </summary>
        /// <remarks>
        /// If there is an active plot device, its visual component will be used.
        /// If there is no active plot device, the next available unassigned
        /// visual component will be used.
        /// If there is no unassigned visual component available, one will be created.
        /// </remarks>
        IRPlotDeviceVisualComponent GetOrCreateMainPlotVisualComponent();

        /// <summary>
        /// Add a visual component to the pool of available components.
        /// </summary>
        /// <param name="visualComponent">Available visual component.</param>
        void RegisterVisualComponent(IRPlotDeviceVisualComponent visualComponent);
    }
}
