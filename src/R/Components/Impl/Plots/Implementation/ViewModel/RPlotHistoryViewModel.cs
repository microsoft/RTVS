// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotHistoryViewModel : BindableBase, IRPlotHistoryViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly ICoreShell _shell;
        private readonly DisposableBag _disposableBag;
        private IRPlotHistoryEntryViewModel _selectedPlot;
        private int _thumbnailSize = DefaultThumbnailSize;

        internal const int MinThumbnailSize = 48;
        internal const int MaxThumbnailSize = 480;
        private const int DefaultThumbnailSize = 96;
        private const int ThumbnailSizeIncrement = 48;

        public RPlotHistoryViewModel(IRPlotManager plotManager, ICoreShell shell) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (shell == null) {
                throw new ArgumentNullException(nameof(shell));
            }

            _plotManager = plotManager;
            _shell = shell;

            _disposableBag = DisposableBag.Create<RPlotHistoryViewModel>()
                .Add(() => _plotManager.DeviceAdded -= DeviceAdded)
                .Add(() => _plotManager.DeviceRemoved -= DeviceRemoved);

            _plotManager.DeviceAdded += DeviceAdded;
            _plotManager.DeviceRemoved += DeviceRemoved;
        }

        public bool AutoHide { get; set; }

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

        public void DecreaseThumbnailSize() {
            _shell.AssertIsOnMainThread();

            if (ThumbnailSize > MinThumbnailSize) {
                ThumbnailSize -= ThumbnailSizeIncrement;
            }
        }

        public void IncreaseThumbnailSize() {
            _shell.AssertIsOnMainThread();

            if (ThumbnailSize < MaxThumbnailSize) {
                ThumbnailSize += ThumbnailSizeIncrement;
            }
        }

        public void SelectEntry(IRPlot plot) {
            _shell.AssertIsOnMainThread();

            foreach (var entry in Entries) {
                if (entry.Plot == plot) {
                    SelectedPlot = entry;
                    break;
                }
            }
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        private void AddOrUpdate(IRPlot plot, BitmapImage plotImage) {
            _shell.AssertIsOnMainThread();

            // A null image means the plot couldn't be rendered.
            // This can happen if an existing plot was resized too small.
            // Don't overwrite the history image in this case, because the
            // existing image in history will be better than no image at all.
            if (plotImage != null) {
                foreach (var entry in Entries) {
                    if (entry.Plot.PlotId == plot.PlotId) {
                        entry.PlotImage = plotImage;
                        return;
                    }
                }
            }

            Entries.Add(new RPlotHistoryEntryViewModel(_plotManager, _shell, plot, plotImage));

            OnPropertyChanged(nameof(ShowWatermark));
        }

        private void Remove(Guid plotId) {
            _shell.AssertIsOnMainThread();

            var t = new List<string>();
            var plot = Entries.SingleOrDefault(p => p.Plot.PlotId == plotId);
            if (plot != null) {
                Entries.Remove(plot);
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        private void RemoveAll(Guid deviceId) {
            _shell.AssertIsOnMainThread();

            foreach (var remove in Entries.Where(e => e.Plot.ParentDevice.DeviceId == deviceId).ToArray()) {
                Entries.Remove(remove);
            }

            OnPropertyChanged(nameof(ShowWatermark));
        }

        private void DeviceRemoved(object sender, RPlotDeviceEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                RemoveAll(e.Device.DeviceId);
            });
            e.Device.PlotAddedOrUpdated -= ActivePlotChanged;
            e.Device.DeviceNumChanged -= DeviceNumChanged;
            e.Device.PlotRemoved -= PlotRemoved;
        }

        private void DeviceAdded(object sender, RPlotDeviceEventArgs e) {
            e.Device.PlotAddedOrUpdated += ActivePlotChanged;
            e.Device.DeviceNumChanged += DeviceNumChanged;
            e.Device.PlotRemoved += PlotRemoved;
        }

        private void PlotRemoved(object sender, RPlotEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                Remove(e.Plot.PlotId);
            });
        }

        private void DeviceNumChanged(object sender, EventArgs e) {
            _shell.DispatchOnUIThread(() => {
                var device = (IRPlotDevice)sender;
                foreach (var entry in Entries.Where(entry => entry.Plot.ParentDevice == device)) {
                    entry.RefreshDeviceName();
                }
            });
        }

        private void ActivePlotChanged(object sender, EventArgs e) {
            _shell.DispatchOnUIThread(() => {
                var device = (IRPlotDevice)sender;
                var plot = device.ActivePlot;
                if (plot != null) {
                    AddOrUpdate(plot, plot.Image);
                }
            });
        }
    }
}
