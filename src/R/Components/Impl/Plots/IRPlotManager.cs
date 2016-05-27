// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Definitions;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotManager : IDisposable {
        IRPlotManagerVisualComponent VisualComponent { get; }

        IRPlotManagerVisualComponent GetOrCreateVisualComponent(IRPlotManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0);

        /// <summary>
        /// A plot message was received, and the state of the plot manager
        /// and visual component has been updated.
        /// </summary>
        /// <remarks>
        /// This is intended to be used by tests, which need to wait on this
        /// event before validating results.
        /// </remarks>
        event EventHandler PlotChanged;

        /// <summary>
        /// Locator mode has started or ended, and the state of the plot
        /// manager and visual component has been updated.
        /// </summary>
        /// <remarks>
        /// This is intended to be used by tests, which need to wait on this
        /// event before validating results.
        /// </remarks>
        event EventHandler LocatorModeChanged;

        /// <summary>
        /// The index of the active plot in the session, -1 if there are no plots.
        /// </summary>
        int ActivePlotIndex { get; }

        /// <summary>
        /// The number of plots in the session.
        /// </summary>
        int PlotCount { get; }

        /// <summary>
        /// Plot commands that associated with this plot manager and its
        /// interactive workflow.
        /// </summary>
        IRPlotCommands Commands { get; }

        /// <summary>
        /// Process an incoming plot message.
        /// </summary>
        /// <param name="plot"></param>
        /// <returns></returns>
        Task LoadPlotAsync(PlotMessage plot);

        /// <summary>
        /// Process an incoming locator message.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct);

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
        Task RemoveAllPlotsAsync();

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
        Task RemoveCurrentPlotAsync();

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
        Task NextPlotAsync();

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
        Task PreviousPlotAsync();

        /// <summary>
        /// Execute code in the session to set the size and resolution of the
        /// plot surface.
        /// </summary>
        /// <param name="pixelWidth">Width in pixels.</param>
        /// <param name="pixelHeight">Height in pixels.</param>
        /// <param name="resolution">Resolution in dpi, ex: 96.</param>
        /// <remarks>
        /// The session will use these new values for all future plot rendering.
        /// If there is an active plot, it will be re-rendered and the session
        /// will send a new plot message.
        /// This is safe to call even if the session isn't running yet. When a
        /// session is connected, the resize will be sent to the session.
        /// Export operations use these values as well.
        /// </remarks>
        /// <exception cref="RPlotManagerException">
        /// An error occurred with the session and the user should be notified.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The session was reset, etc. this can be silenced.
        /// </exception>
        Task ResizeAsync(int pixelWidth, int pixelHeight, int resolution);

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
        Task ExportToBitmapAsync(string deviceName, string outputFilePath);

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
        Task ExportToMetafileAsync(string outputFilePath);

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
        Task ExportToPdfAsync(string outputFilePath);

        /// <summary>
        /// End the locator mode with a locator result that indicate the user
        /// wants to end the locator session.
        /// </summary>
        void EndLocatorMode();

        /// <summary>
        /// End the locator mode with the specified result.
        /// </summary>
        /// <param name="result"></param>
        void EndLocatorMode(LocatorResult result);

        /// <summary>
        /// Indicate whether locator mode is active or not.
        /// </summary>
        bool IsInLocatorMode { get; }
    }
}
