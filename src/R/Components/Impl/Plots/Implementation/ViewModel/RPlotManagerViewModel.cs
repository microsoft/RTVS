// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    internal class RPlotManagerViewModel : BindableBase, IRPlotManagerViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly DelayedAsyncAction _resizeAction = new DelayedAsyncAction(250);
        private BitmapImage _plotImage;
        private bool _locatorMode;
        private bool _showWatermark;
        private bool _showError;

        public RPlotManagerViewModel(IRPlotManager plotManager) {
            _plotManager = plotManager;
            _showWatermark = true;
        }

        public BitmapImage PlotImage {
            get { return _plotImage; }
            private set { SetProperty(ref _plotImage, value); }
        }

        public bool LocatorMode {
            get { return _locatorMode; }
            private set { SetProperty(ref _locatorMode, value); }
        }

        public bool ShowWatermark {
            get { return _showWatermark; }
            private set { SetProperty(ref _showWatermark, value); }
        }

        public bool ShowError {
            get { return _showError; }
            private set { SetProperty(ref _showError, value); }
        }

        public void LoadPlot(BitmapImage image) {
            PlotImage = image;
            ShowWatermark = false;
            ShowError = false;
        }

        public void Clear() {
            PlotImage = null;
            ShowWatermark = true;
            ShowError = false;
        }

        public void Error() {
            PlotImage = null;
            ShowWatermark = false;
            ShowError = true;
        }

        public void SetLocatorMode(bool locatorMode) {
            LocatorMode = locatorMode;
        }

        public async Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution) {
            await _plotManager.ResizeAsync(pixelWidth, pixelHeight, resolution);
        }

        public void ResizePlotAfterDelay(int pixelWidth, int pixelHeight, int resolution) {
            // Throttle rendering of plot while user is resizing the window.
            // Plot rendering isn't fast enough to keep up with live resizing,
            // which is what happens with undocked VS tool windows.
            _resizeAction.Invoke(() => ResizePlotAsync(pixelWidth, pixelHeight, resolution));
        }

        public void ClickPlot(int pixelX, int pixelY) {
            if (LocatorMode) {
                var result = LocatorResult.CreateClicked(pixelX, pixelY);
                _plotManager.EndLocatorMode(result);
            }
        }
    }
}
