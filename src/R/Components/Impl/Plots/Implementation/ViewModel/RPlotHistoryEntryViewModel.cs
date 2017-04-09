// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotHistoryEntryViewModel : BindableBase, IRPlotHistoryEntryViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly IMainThread _mainThread;
        private readonly IRPlot _plot;
        private BitmapImage _plotImage;
        private string _deviceName;

        public RPlotHistoryEntryViewModel(IRPlotManager plotManager, IMainThread mainThread, IRPlot plot, BitmapImage plotImage) {
            Check.ArgumentNull(nameof(plotManager), plotManager);
            Check.ArgumentNull(nameof(mainThread), mainThread);
            Check.ArgumentNull(nameof(plot), plot);
            Check.ArgumentNull(nameof(plotImage), plotImage);

            _plotManager = plotManager;
            _mainThread = mainThread;
            _plot = plot;
            _plotImage = plotImage;

            RefreshDeviceName();
        }

        public IRPlot Plot => _plot;

        public BitmapImage PlotImage {
            get { return _plotImage; }
            set { SetProperty(ref _plotImage, value); }
        }

        public string DeviceName {
            get { return _deviceName; }
            private set { SetProperty(ref _deviceName, value); }
        }

        public void RefreshDeviceName() {
            _mainThread.Assert();
            DeviceName = string.Format(CultureInfo.CurrentCulture, Resources.Plots_DeviceName, _plot.ParentDevice.DeviceNum);
        }

        public async Task ActivatePlotAsync() {
            await _plotManager.ActivatePlotAsync(_plot);
        }
    }
}
