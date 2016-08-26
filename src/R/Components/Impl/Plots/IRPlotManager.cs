// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotManager : IDisposable {
        IRInteractiveWorkflow InteractiveWorkflow { get; }

        IRPlotDeviceVisualComponent GetOrCreateVisualComponent(IRPlotDeviceVisualComponentContainerFactory visualComponentContainerFactory, int instanceId);
        IRPlotHistoryVisualComponent GetOrCreateVisualComponent(IRPlotHistoryVisualComponentContainerFactory visualComponentContainerFactory, int instanceId);

        /// <summary>
        /// Visual component for the global plot history, or <c>null</c> if it
        /// hasn't been created yet.
        /// </summary>
        IRPlotHistoryVisualComponent HistoryVisualComponent { get; }

        /// <summary>
        /// Global history of all plots in all ide devices.
        /// </summary>
        IRPlotHistoryViewModel History { get; }

        /// <summary>
        /// The active device. This is updated on every session mutated event
        /// so it's accurate even if the user changes the active device using
        /// dev.set(). Wait for the <seealso cref="ActiveDeviceChanged"/> event
        /// if you need to check this value after running code in the session.
        /// </summary>
        Guid ActiveDeviceId { get; }

        event EventHandler<EventArgs> DeviceCreateMessageReceived;
        event EventHandler<EventArgs> DeviceDestroyMessageReceived;
        event EventHandler<EventArgs> PlotMessageReceived;

        event EventHandler<EventArgs> LocatorModeChanged;

        /// <summary>
        /// The active device has changed. This is checked for on every session 
        /// mutated event and only fired when different from the previous value.
        /// </summary>
        event EventHandler<EventArgs> ActiveDeviceChanged;

        /// <summary>
        /// Process an incoming plot message from the host.
        /// </summary>
        /// <param name="plot"></param>
        Task LoadPlotAsync(PlotMessage plot);

        /// <summary>
        /// Process an incoming locator message from the host.
        /// </summary>
        /// <param name="deviceId">Id of device whose locator function was called.</param>
        /// <param name="ct"></param>
        Task<LocatorResult> StartLocatorModeAsync(Guid deviceId, CancellationToken ct);

        /// <summary>
        /// Process an incoming device creation message from the host.
        /// This assigns the new device to an available visual component (creating one if necessary).
        /// </summary>
        /// <param name="deviceId">Id of device that is being created.</param>
        /// <returns>
        /// Properties of the visual component assigned to the device. The host
        /// uses this to set the device size and resolution.
        /// </returns>
        Task<PlotDeviceProperties> DeviceCreatedAsync(Guid deviceId);

        /// <summary>
        /// Process an incoming device destroy message from the host.
        /// This unassigns the device from its visual component, which will be
        /// recycled for the next device that is created.
        /// </summary>
        /// <param name="deviceId">Id of device that is being destroyed.</param>
        Task DeviceDestroyedAsync(Guid deviceId);

        /// <summary>
        /// Show the visual component for the specified device.
        /// Does nothing if a visual component for the device was not found.
        /// </summary>
        /// <param name="deviceId">Device to show.</param>
        Task ShowDeviceAsync(Guid deviceId);

        /// <summary>
        /// Execute code in the session to remove all plots.
        /// </summary>
        /// <remarks>
        /// This does not directly update the state of the plot manager and
        /// its visual component. The session will send a new plot message
        /// which will cause the update.
        /// </remarks>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task RemoveAllPlotsAsync(Guid deviceId);

        /// <summary>
        /// Execute code in the session to remove the current plot.
        /// </summary>
        /// <remarks>
        /// This does not directly update the state of the plot manager and
        /// its visual component. The session will send a new plot message
        /// which will cause the update.
        /// </remarks>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task RemovePlotAsync(Guid deviceId, Guid plotId);

        /// <summary>
        /// Execute code in the session to activate a graphics device
        /// and render the specified plot.
        /// </summary>
        /// <param name="deviceId">Device to activate.</param>
        /// <param name="plotId"></param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ActivatePlotAsync(Guid deviceId, Guid plotId);

        /// <summary>
        /// Execute code in the session to change the active plot to the next
        /// plot, if available.
        /// </summary>
        /// <remarks>
        /// This does not directly update the state of the plot manager and
        /// its visual component. The session will send a new plot message
        /// which will cause the update.
        /// </remarks>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task NextPlotAsync(Guid deviceId);

        /// <summary>
        /// Execute code in the session to change the active plot to the previous
        /// plot, if available.
        /// </summary>
        /// <remarks>
        /// This does not directly update the state of the plot manager and
        /// its visual component. The session will send a new plot message
        /// which will cause the update.
        /// </remarks>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task PreviousPlotAsync(Guid deviceId);

        /// <summary>
        /// Execute code in the session to set the size and resolution of the
        /// plot surface.
        /// </summary>
        /// <param name="pixelWidth">Width in pixels.</param>
        /// <param name="pixelHeight">Height in pixels.</param>
        /// <param name="resolution">Resolution in dpi, ex: 96.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ResizeAsync(Guid deviceId, int pixelWidth, int pixelHeight, int resolution);

        /// <summary>
        /// Execute code in the session to export the active plot as a bitmap.
        /// </summary>
        /// <param name="deviceName">Device to use: png, bmp, jpeg, tiff.</param>
        /// <param name="outputFilePath">File to save to.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ExportToBitmapAsync(Guid deviceId, Guid plotId, string deviceName, string outputFilePath, int pixelWidth, int pixelHeight, int resolution);

        /// <summary>
        /// Execute code in the session to export the active plot as a metafile.
        /// </summary>
        /// <param name="outputFilePath">File to save to.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ExportToMetafileAsync(Guid deviceId, Guid plotId, string outputFilePath, double inchWidth, double inchHeight, int resolution);

        /// <summary>
        /// Execute code in the session to export the active plot as a PDF.
        /// </summary>
        /// <param name="outputFilePath">File to save to.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ExportToPdfAsync(Guid deviceId, Guid plotId, string outputFilePath, double inchWidth, double inchHeight);

        /// <summary>
        /// Execute code in the session to change the active graphics device.
        /// </summary>
        /// <param name="deviceId">Device to make active.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ActivateDeviceAsync(Guid deviceId);

        /// <summary>
        /// Execute code in the session to create a new graphics device and
        /// associate it with the a visual component instance (if specified).
        /// </summary>
        /// <param name="existingInstanceId">
        /// Visual component instance to use, or -1 to use any available or
        /// create a new one.
        /// </param>
        Task NewDeviceAsync(int existingInstanceId);

        /// <summary>
        /// Execute code in the session to copy a plot from one device to another.
        /// </summary>
        /// <param name="sourceDeviceId">Device to copy from.</param>
        /// <param name="sourcePlotId">Plot to copy.</param>
        /// <param name="targetDeviceId">Device to copy to.</param>
        Task CopyPlotAsync(Guid sourceDeviceId, Guid sourcePlotId, Guid targetDeviceId);

        /// <summary>
        /// Add a visual component to the pool of available components.
        /// </summary>
        /// <param name="visualComponent">Available visual component.</param>
        void RegisterVisualComponent(IRPlotDeviceVisualComponent visualComponent);

        /// <summary>
        /// Get the view model for the specified device.
        /// Used by tests to validate the state of a device.
        /// </summary>
        /// <param name="deviceId">Device view model to retrieve.</param>
        /// <returns>Device view model.</returns>
        IRPlotDeviceViewModel GetDeviceViewModel(Guid deviceId);
    }
}
