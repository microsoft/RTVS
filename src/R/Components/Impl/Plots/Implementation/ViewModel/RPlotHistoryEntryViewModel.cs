// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotHistoryEntryViewModel : BindableBase, IRPlotHistoryEntryViewModel {
        private readonly IRPlotManager _plotManager;
        private BitmapImage _plotImage;

        public RPlotHistoryEntryViewModel(IRPlotManager plotManager, string deviceName, Guid deviceId, Guid plotId, int? sessionProcessId, BitmapImage plotImage) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (deviceName == null) {
                throw new ArgumentNullException(nameof(deviceName));
            }

            _plotManager = plotManager;
            DeviceName = deviceName;
            DeviceId = deviceId;
            PlotId = plotId;
            SessionProcessId = sessionProcessId;
            _plotImage = plotImage;
        }

        public BitmapImage PlotImage {
            get { return _plotImage; }
            set { SetProperty(ref _plotImage, value); }
        }

        public Guid PlotId { get; }

        public Guid DeviceId { get; }

        public int? SessionProcessId { get; }

        public string DeviceName { get; }

        public async Task ActivatePlotAsync() {
            await _plotManager.ActivatePlotAsync(DeviceId, PlotId);
        }
    }
}
