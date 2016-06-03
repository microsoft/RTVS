// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Media.Imaging;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotManagerVisualComponent : IVisualComponent {
        void LoadPlot(BitmapImage image);
        void Clear();
        void Error();
        void SetLocatorMode(bool locatorMode);
        void Click(int pixelX, int pixelY);
    }
}
