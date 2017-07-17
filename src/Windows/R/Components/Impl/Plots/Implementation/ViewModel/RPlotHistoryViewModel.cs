// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotHistoryViewModel : BindableBase, IRPlotHistoryViewModel {
        private readonly RPlotHistoryControl _control;
        private readonly IRPlotManager _plotManager;
        private readonly IMainThread _mainThread;
        private readonly DisposableBag _disposableBag;
        private int _thumbnailSize = DefaultThumbnailSize;

        internal const int MinThumbnailSize = 48;
        internal const int MaxThumbnailSize = 480;
        private const int DefaultThumbnailSize = 96;
        private const int ThumbnailSizeIncrement = 48;

        public RPlotHistoryViewModel(RPlotHistoryControl control, IRPlotManager plotManager, IMainThread mainThread) {
            Check.ArgumentNull(nameof(control), control);
            Check.ArgumentNull(nameof(plotManager), plotManager);
            Check.ArgumentNull(nameof(mainThread), mainThread);

            _control = control;
            _plotManager = plotManager;
            _mainThread = mainThread;

            _disposableBag = DisposableBag.Create<RPlotHistoryViewModel>()
                .Add(() => _plotManager.DeviceAdded -= DeviceAdded)
                .Add(() => _plotManager.DeviceRemoved -= DeviceRemoved);

            _plotManager.DeviceAdded += DeviceAdded;
            _plotManager.DeviceRemoved += DeviceRemoved;

            foreach (var group in _plotManager.GetAllPlots().GroupBy(p => p.ParentDevice)) {
                SubscribeDeviceEvents(group.Key);
                foreach (var plot in group) {
                    Entries.Add(new RPlotHistoryEntryViewModel(_plotManager, _mainThread, plot, plot.Image));
                }
            }
        }

        public bool AutoHide { get; set; }

        public bool ShowWatermark => Entries.Count == 0;

        public ObservableCollection<IRPlotHistoryEntryViewModel> Entries { get; } =
            new ObservableCollection<IRPlotHistoryEntryViewModel>();

        public IEnumerable<IRPlotHistoryEntryViewModel> SelectedPlots 
            => _control.HistoryListView.SelectedItems.Cast<IRPlotHistoryEntryViewModel>();

        public int ThumbnailSize {
            get => _thumbnailSize;
            set => SetProperty(ref _thumbnailSize, value);
        }

        public void DecreaseThumbnailSize() {
            _mainThread.Assert();

            if (ThumbnailSize > MinThumbnailSize) {
                ThumbnailSize -= ThumbnailSizeIncrement;
            }
        }

        public void IncreaseThumbnailSize() {
            _mainThread.Assert();

            if (ThumbnailSize < MaxThumbnailSize) {
                ThumbnailSize += ThumbnailSizeIncrement;
            }
        }

        public void SelectEntry(IRPlot plot) {
            _mainThread.Assert();
            _control.HistoryListView.SelectedItem = Entries.FirstOrDefault(e => e.Plot == plot);
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        private void AddOrUpdate(IRPlot plot, object plotImage) {
            _mainThread.Assert();

            // Some error messages coming from the host may not have a plot id
            // associated with them. We never want to add a history entry in that case.
            if (plot.PlotId == Guid.Empty) {
                return;
            }

            var entry = Entries.SingleOrDefault(p => p.Plot.PlotId == plot.PlotId);
            if (entry != null) {
                // A null image means the plot couldn't be rendered.
                // This can happen if an existing plot was resized too small.
                // Don't overwrite the history image in this case, because the
                // existing image in history will be better than no image at all.
                if (plotImage != null) {
                    entry.PlotImage = plotImage;
                }
            } else {
                Entries.Add(new RPlotHistoryEntryViewModel(_plotManager, _mainThread, plot, plotImage));
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        private void Remove(Guid plotId) {
            _mainThread.Assert();

            var t = new List<string>();
            var plot = Entries.SingleOrDefault(p => p.Plot.PlotId == plotId);
            if (plot != null) {
                Entries.Remove(plot);
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        private void RemoveAll(Guid deviceId) {
            _mainThread.Assert();

            foreach (var remove in Entries.Where(e => e.Plot.ParentDevice.DeviceId == deviceId).ToArray()) {
                Entries.Remove(remove);
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        private void DeviceRemoved(object sender, RPlotDeviceEventArgs e) {
            _mainThread.Post(() => {
                RemoveAll(e.Device.DeviceId);
            });
            UnsubscribeDeviceEvents(e.Device);
        }

        private void DeviceAdded(object sender, RPlotDeviceEventArgs e) {
            SubscribeDeviceEvents(e.Device);
        }

        private void SubscribeDeviceEvents(IRPlotDevice device) {
            device.PlotAddedOrUpdated += ActivePlotChanged;
            device.Cleared += DeviceCleared;
            device.DeviceNumChanged += DeviceNumChanged;
            device.PlotRemoved += PlotRemoved;
        }

        private void UnsubscribeDeviceEvents(IRPlotDevice device) {
            device.PlotAddedOrUpdated -= ActivePlotChanged;
            device.Cleared -= DeviceCleared;
            device.DeviceNumChanged -= DeviceNumChanged;
            device.PlotRemoved -= PlotRemoved;
        }

        private void PlotRemoved(object sender, RPlotEventArgs e) {
            _mainThread.Post(() => {
                Remove(e.Plot.PlotId);
            });
        }

        private void DeviceNumChanged(object sender, EventArgs e) {
            _mainThread.Post(() => {
                var device = (IRPlotDevice)sender;
                foreach (var entry in Entries.Where(entry => entry.Plot.ParentDevice == device)) {
                    entry.RefreshDeviceName();
                }
            });
        }

        private void ActivePlotChanged(object sender, EventArgs e) {
            _mainThread.Post(() => {
                var device = (IRPlotDevice)sender;
                var plot = device.ActivePlot;
                if (plot != null) {
                    AddOrUpdate(plot, plot.Image);
                }
            });
        }

        private void DeviceCleared(object sender, EventArgs e) {
            _mainThread.Post(() => {
                var device = (IRPlotDevice)sender;
                RemoveAll(device.DeviceId);
            });
        }
    }
}
