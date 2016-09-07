// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotHistoryEntryViewModel : BindableBase, IRPlotHistoryEntryViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly ICoreShell _shell;
        private readonly IRPlot _plot;
        private BitmapImage _plotImage;
        private string _deviceName;

        public RPlotHistoryEntryViewModel(IRPlotManager plotManager, ICoreShell shell, IRPlot plot, BitmapImage plotImage) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (shell == null) {
                throw new ArgumentNullException(nameof(shell));
            }

            if (plot == null) {
                throw new ArgumentNullException(nameof(plot));
            }

            _plotManager = plotManager;
            _shell = shell;
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
            _shell.AssertIsOnMainThread();

            DeviceName = string.Format(CultureInfo.CurrentUICulture, Resources.Plots_DeviceName, _plot.ParentDevice.DeviceNum);
        }

        public async Task ActivatePlotAsync() {
            await _plotManager.ActivatePlotAsync(_plot);
        }
    }
}
