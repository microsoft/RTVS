// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.R.Components.Plots.ViewModel {
    public interface IRPlotHistoryViewModel : IDisposable {
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
        IEnumerable<IRPlotHistoryEntryViewModel> SelectedPlots { get; }

        /// <summary>
        /// Decrease size of thumbnails by 48 wpf units.
        /// </summary>
        void DecreaseThumbnailSize();

        /// <summary>
        /// Increase size of thumbnails by 48 wpf units.
        /// </summary>
        void IncreaseThumbnailSize();

        /// <summary>
        /// Select the entry for the specified plot.
        /// </summary>
        /// <param name="plot">Plot to select.</param>
        void SelectEntry(IRPlot plot);
    }
}
