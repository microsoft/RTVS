// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots.ViewModel {
    public interface IRPlotHistoryViewModel {
        /// <summary>
        /// When a plot is activated by the user, the visual component is closed
        /// automatically. This is extra convenience for users that want to float
        /// the plot history window and have it closed as they double-click to activate
        /// a plot, saving the extra step of getting the window out of the way.
        /// </summary>
        bool AutoHide { get; set; }

        /// <summary>
        /// Size (width and height in wpf units) for the thumbnails.
        /// </summary>
        int ThumbnailSize { get; set; }

        /// <summary>
        /// All plots from all ide devices.
        /// </summary>
        ObservableCollection<IRPlotHistoryEntryViewModel> Entries { get; }

        /// <summary>
        /// The currently selected plot, ie. commands such as activate,
        /// remove apply to this plot.
        /// </summary>
        IRPlotHistoryEntryViewModel SelectedPlot { get; set; }

        /// <summary>
        /// Decrease size of thumbnails by 48 wpf units.
        /// </summary>
        void DecreaseThumbnailSize();

        /// <summary>
        /// Increase size of thumbnails by 48 wpf units.
        /// </summary>
        void IncreaseThumbnailSize();

        /// <summary>
        /// Update the plot entries when a new plot message is received from the host.
        /// Existing plots will have their image replaced, unless there was an error rendering the image.
        /// </summary>
        /// <param name="deviceName">Localized name for the device.</param>
        /// <param name="deviceId">Internal id for the device.</param>
        /// <param name="plotId">Internal id for the plot.</param>
        /// <param name="sessionProcessId">Process id for the device's session.</param>
        /// <param name="plotImage">Bitmap image for the plot (may be <c>null</c>).</param>
        void AddOrUpdate(string deviceName, Guid deviceId, Guid plotId, int? sessionProcessId, BitmapImage plotImage);

        /// <summary>
        /// Remove the plot from the history.
        /// </summary>
        /// <param name="plotId">Internal id for the plot to remove.</param>
        void Remove(Guid plotId);

        /// <summary>
        /// Remove all plots from the history for the specified device.
        /// </summary>
        /// <param name="deviceId">Device for which to remove plots.</param>
        void RemoveAll(Guid deviceId);

        /// <summary>
        /// Remove all plots from the history.
        /// </summary>
        void Clear();
    }
}
