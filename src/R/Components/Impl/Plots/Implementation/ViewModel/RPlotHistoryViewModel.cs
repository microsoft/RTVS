// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotHistoryViewModel : BindableBase, IRPlotHistoryViewModel {
        private readonly IRPlotManager _plotManager;
        private IRPlotHistoryEntryViewModel _selectedPlot;
        private int _thumbnailSize = DefaultThumbnailSize;

        internal const int MinThumbnailSize = 48;
        internal const int MaxThumbnailSize = 480;

        private const int DefaultThumbnailSize = 96;
        private const int ThumbnailSizeIncrement = 48;

        public RPlotHistoryViewModel(IRPlotManager plotManager) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            _plotManager = plotManager;
        }

        public bool ShowWatermark => Entries.Count == 0;

        public ObservableCollection<IRPlotHistoryEntryViewModel> Entries { get; } = new ObservableCollection<IRPlotHistoryEntryViewModel>();

        public IRPlotHistoryEntryViewModel SelectedPlot {
            get { return _selectedPlot; }
            set { SetProperty(ref _selectedPlot, value); }
        }

        public int ThumbnailSize {
            get { return _thumbnailSize; }
            set { SetProperty(ref _thumbnailSize, value); }
        }

        public bool AutoHide { get; set; }

        public void DecreaseThumbnailSize() {
            if (ThumbnailSize > MinThumbnailSize) {
                ThumbnailSize -= ThumbnailSizeIncrement;
            }
        }

        public void IncreaseThumbnailSize() {
            if (ThumbnailSize < MaxThumbnailSize) {
                ThumbnailSize += ThumbnailSizeIncrement;
            }
        }

        public void AddOrUpdate(string deviceName, Guid deviceId, Guid plotId, int? sessionProcessId, BitmapImage plotImage) {
            // A null image means the plot couldn't be rendered.
            // This can happen if an existing plot was resized too small.
            // Don't overwrite the history image in this case, because the
            // existing image in history will be better than no image at all.
            if (plotImage != null) {
                foreach (var entry in Entries) {
                    if (entry.PlotId == plotId) {
                        entry.PlotImage = plotImage;
                        return;
                    }
                }
            }

            Entries.Add(new RPlotHistoryEntryViewModel(_plotManager, deviceName, deviceId, plotId, sessionProcessId, plotImage));

            OnPropertyChanged(nameof(ShowWatermark));
        }

        public void Remove(Guid plotId) {
            var t = new List<string>();
            var plot = Entries.SingleOrDefault(p => p.PlotId == plotId);
            if (plot != null) {
                Entries.Remove(plot);
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        public void RemoveAll(Guid deviceId) {
            foreach (var remove in Entries.Where(e => e.DeviceId == deviceId).ToArray()) {
                Entries.Remove(remove);
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        public void Clear() {
            Entries.Clear();

            OnPropertyChanged(nameof(ShowWatermark));
        }
    }
}
