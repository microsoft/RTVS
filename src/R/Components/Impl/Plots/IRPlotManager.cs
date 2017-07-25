// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotManager : IDisposable {
        IRInteractiveWorkflow InteractiveWorkflow { get; }

        /// <summary>
        /// The active device. This is updated on every session mutated event
        /// so it's accurate even if the user changes the active device using
        /// dev.set(). Wait for the <seealso cref="ActiveDeviceChanged"/> event
        /// if you need to check this value after running code in the session.
        /// </summary>
        IRPlotDevice ActiveDevice { get; }

        /// <summary>
        /// The active device has changed. This is checked for on every session 
        /// mutated event and only fired when different from the previous value.
        /// </summary>
        event EventHandler<RPlotDeviceEventArgs> ActiveDeviceChanged;

        /// <summary>
        /// A device was created.
        /// </summary>
        event EventHandler<RPlotDeviceEventArgs> DeviceAdded;

        /// <summary>
        /// A device was removed.
        /// </summary>
        event EventHandler<RPlotDeviceEventArgs> DeviceRemoved;

        /// <summary>
        /// Process an incoming plot message from the host.
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="cancellationToken"></param>
        Task LoadPlotAsync(PlotMessage plot, CancellationToken cancellationToken);

        /// <summary>
        /// Process an incoming locator message from the host.
        /// </summary>
        /// <param name="deviceId">Id of device whose locator function was called.</param>
        /// <param name="cancellationToken"></param>
        Task<LocatorResult> StartLocatorModeAsync(Guid deviceId, CancellationToken cancellationToken);

        /// <summary>
        /// End the locator mode with the specified result.
        /// </summary>
        void EndLocatorMode(IRPlotDevice device, LocatorResult result);

        /// <summary>
        /// Process an incoming device creation message from the host.
        /// This assigns the new device to an available visual component (creating one if necessary).
        /// </summary>
        /// <param name="deviceId">Id of device that is being created.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Properties of the visual component assigned to the device. The host
        /// uses this to set the device size and resolution.
        /// </returns>
        Task<PlotDeviceProperties> DeviceCreatedAsync(Guid deviceId, CancellationToken cancellationToken);

        /// <summary>
        /// Process an incoming device destroy message from the host.
        /// This unassigns the device from its visual component, which will be
        /// recycled for the next device that is created.
        /// </summary>
        /// <param name="deviceId">Id of device that is being destroyed.</param>
        /// <param name="cancellationToken"></param>
        Task DeviceDestroyedAsync(Guid deviceId, CancellationToken cancellationToken);

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
        Task RemoveAllPlotsAsync(IRPlotDevice device);

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
        Task RemovePlotAsync(IRPlot plot);

        /// <summary>
        /// Execute code in the session to activate a graphics device
        /// and render the specified plot.
        /// </summary>
        /// <param name="plot">Plot to activate.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ActivatePlotAsync(IRPlot plot);

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
        Task NextPlotAsync(IRPlotDevice device);

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
        Task PreviousPlotAsync(IRPlotDevice device);

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
        Task ResizeAsync(IRPlotDevice device, int pixelWidth, int pixelHeight, int resolution);

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
        Task ExportToBitmapAsync(IRPlot plot, string deviceName, string outputFilePath, int pixelWidth, int pixelHeight, int resolution);

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
        Task ExportToMetafileAsync(IRPlot plot, string outputFilePath, double inchWidth, double inchHeight, int resolution);

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
        Task ExportToPdfAsync(IRPlot plot, string pdfDevice, string paper, string outputFilePath, double inchWidth, double inchHeight);

        /// <summary>
        /// Execute code in the session to change the active graphics device.
        /// </summary>
        /// <param name="device">Device to make active.</param>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ActivateDeviceAsync(IRPlotDevice device);

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
        /// Execute code in the session to copy or move a plot from one device to another.
        /// </summary>
        /// <param name="sourceDeviceId"></param>
        /// <param name="sourcePlotId"></param>
        /// <param name="targetDevice"></param>
        /// <param name="isMove"></param>
        Task CopyOrMovePlotFromAsync(Guid sourceDeviceId, Guid sourcePlotId, IRPlotDevice targetDevice, bool isMove);

        /// <summary>
        /// Get all the plots stored in all the plot devices.
        /// </summary>
        /// <returns></returns>
        IRPlot[] GetAllPlots();
    }
}
