// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Components.Plots.ViewModel {
    public interface IRPlotDeviceViewModel {
        event EventHandler DeviceNameChanged;

        /// <summary>
        /// A plot message was received, and the state of the plot manager
        /// and visual component has been updated.
        /// </summary>
        event EventHandler PlotChanged;

        /// <summary>
        /// Locator mode has started or ended, and the state of the plot
        /// manager and visual component has been updated.
        /// </summary>
        event EventHandler LocatorModeChanged;

        /// <summary>
        /// Bitmap for the active plot.
        /// </summary>
        object PlotImage { get; }

        /// <summary>
        /// Locator mode is active, ie. we are waiting on the user
        /// to either click on the plot, or click on End Locator command.
        /// </summary>
        bool LocatorMode { get; }

        /// <summary>
        /// <c>true</c> if there are no plots in the device.
        /// </summary>
        bool ShowWatermark { get; }

        /// <summary>
        /// <c>true</c> if there is an active plot, but it could not be rendered.
        /// </summary>
        bool ShowError { get; }

        /// <summary>
        /// Localized display name for the device, includes the device number
        /// that is exposed to the user via R dev.cur().
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// <c>true</c> if this device is active, ie. plotting operations will
        /// go to this device.
        /// </summary>
        bool IsDeviceActive { get; }

        /// <summary>
        /// Visual component window instance id.
        /// </summary>
        int InstanceId { get; }

        IRPlotDevice Device { get; }

        /// <summary>
        /// Initialize this view model for the specified device.
        /// </summary>
        /// <param name="device">Device to set.</param>
        void Assign(IRPlotDevice device);

        /// <summary>
        /// Cleanup this view model so it can be reused later for a different device.
        /// </summary>
        void Unassign();

        /// <summary>
        /// Resize the plot.
        /// </summary>
        /// <param name="pixelWidth">Width in pixels.</param>
        /// <param name="pixelHeight">Height in pixels.</param>
        /// <param name="resolution">Resolution in dpi, ex: 96.</param>
        /// <remarks>
        /// The device will use these new values for all future plot rendering.
        /// If there is an active plot, it will be re-rendered and the device
        /// will send a new plot message.
        /// This is safe to call even if the view model isn't assigned to a device yet.
        /// </remarks>
        Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution);

        /// <summary>
        /// Set the click result when in locator mode.
        /// </summary>
        /// <param name="pixelX"></param>
        /// <param name="pixelY"></param>
        void ClickPlot(int pixelX, int pixelY);

        /// <summary>
        /// Copy the specified plot from another device to this device.
        /// </summary>
        /// <param name="sourceDeviceId">Device to copy from.</param>
        /// <param name="sourcePlotId">Plot to copy.</param>
        /// <param name="isMove"><c>true</c> to delete the source plot after move.</param>
        Task CopyPlotFromAsync(Guid sourceDeviceId, Guid sourcePlotId, bool isMove);
    }
}
