// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots.ViewModel {
    public interface IRPlotManagerViewModel : IDisposable {
        BitmapImage PlotImage { get; }
        bool LocatorMode { get; }
        bool ShowWatermark { get; }
        bool ShowError { get; }
        void LoadPlot(BitmapImage image);
        void Clear();
        void Error();
        void SetLocatorMode(bool locatorMode);
        Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution);
        void ResizePlotAfterDelay(int pixelWidth, int pixelHeight, int resolution);
        void ClickPlot(int pixelX, int pixelY);
    }
}
