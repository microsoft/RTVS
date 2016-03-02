// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.Plots {
    /// <summary>
    /// Plot content provider to load and consume plot content
    /// </summary>
    internal interface IPlotContentProvider : IDisposable {
        /// <summary>
        /// Event raised when UIElement is loaded, content presenter listens to this event
        /// </summary>
        event EventHandler<PlotChangedEventArgs> PlotChanged;

        /// <summary>
        /// Load a file to create plot UIElement
        /// </summary>
        /// <param name="filePath">path to a file</param>
        void LoadFile(string filePath);

        /// <summary>
        /// Export plot as an image.
        /// </summary>
        /// <param name="fileName">Destination file path.</param>
        /// <param name="deviceName">Name of R device to use (png, bmp, tiff, jpeg).</param>
        void ExportAsImage(string fileName, string deviceName);

        /// <summary>
        /// Export plot as a PDF.
        /// </summary>
        /// <param name="fileName">Destination file path.</param>
        void ExportAsPdf(string fileName);

        /// <summary>
        /// Copy plot to clipboard in bitmap format.
        /// </summary>
        void CopyToClipboardAsBitmap();

        /// <summary>
        /// Copy plot to clipboard in enhanced metafile format.
        /// </summary>
        void CopyToClipboardAsMetafile();

        /// <summary>
        /// Resize the current plot or set the default size for future plots.
        /// </summary>
        /// <param name="pixelWidth">Width in pixels.</param>
        /// <param name="pixelHeight">Height in pixels.</param>
        Task ResizePlotAsync(int pixelWidth, int pixelHeight);

        /// <summary>
        /// Navigate to the next plot in the plot history.
        /// </summary>
        Task NextPlotAsync();

        /// <summary>
        /// Navigate to the previous plot in the plot history.
        /// </summary>
        Task PreviousPlotAsync();

        /// <summary>
        /// Get the plot history information.
        /// </summary>
        /// <returns>Tuple of active index (0-based, -1 for none) and number of plots.</returns>
        Task<PlotHistoryInfo> GetHistoryInfoAsync();

        /// <summary>
        /// Clear all plots from the plot history.
        /// </summary>
        Task ClearAllAsync();

        /// <summary>
        /// Remove the current plot from the plot history.
        /// </summary>
        Task RemoveCurrentPlotAsync();
    }

    /// <summary>
    /// provide data ralated to PlotChanged event
    /// </summary>
    internal class PlotChangedEventArgs : EventArgs {
        /// <summary>
        /// new plot UIElement
        /// </summary>
        public UIElement NewPlotElement { get; set; }
    }

    internal class PlotHistoryInfo {
        public PlotHistoryInfo() :
            this(-1, 0) {
        }

        public PlotHistoryInfo(int activePlotIndex, int plotCount) {
            this.ActivePlotIndex = activePlotIndex;
            this.PlotCount = plotCount;
        }

        public int ActivePlotIndex { get; set; }

        public int PlotCount { get; set; }
    }
}
