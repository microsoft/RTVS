// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.View.DesignTime {
#if DEBUG
    class DesignTimeRPlotHistoryViewModel : IRPlotHistoryViewModel {
        public ObservableCollection<IRPlotHistoryEntryViewModel> Entries { get; } = new ObservableCollection<IRPlotHistoryEntryViewModel>();

        public Func<IRPlotHistoryEntryViewModel, Task> PlotSelector { get; set; }

        public IEnumerable<IRPlotHistoryEntryViewModel> SelectedPlots { get; set; }

        public bool AutoHide { get; set; }
        public int ThumbnailSize { get; set; } = 96;

        public Action<int> ThumbnailSizeUpdater { get; set; }

        public DesignTimeRPlotHistoryViewModel() {
            Entries.Add(new DesignTimeRPlotHistoryEntryViewModel() {
                DeviceName = "Device 1",
                PlotImage = new BitmapImage(new Uri("https://i3-vso.sec.s-msft.com/dn469161.1-Xamarin-swimlane-image.png")),
            });
            Entries.Add(new DesignTimeRPlotHistoryEntryViewModel() {
                DeviceName = "Device 1",
                PlotImage = new BitmapImage(new Uri("https://i3-vso.sec.s-msft.com/dynimg/IC854673.png")),
            });
            Entries.Add(new DesignTimeRPlotHistoryEntryViewModel() {
                DeviceName = "Device 1",
                PlotImage = new BitmapImage(new Uri("https://i3-vso.sec.s-msft.com/dynimg/IC854672.png")),
            });
            Entries.Add(new DesignTimeRPlotHistoryEntryViewModel() {
                DeviceName = "Device 2",
                PlotImage = new BitmapImage(new Uri("https://i3-vso.sec.s-msft.com/dynimg/IC854675.png")),
            });
        }

        public void DecreaseThumbnailSize() {
        }

        public void IncreaseThumbnailSize() {
        }

        public void SelectEntry(IRPlot plot) {
        }

        public void Dispose() {
        }
    }
#endif
}
