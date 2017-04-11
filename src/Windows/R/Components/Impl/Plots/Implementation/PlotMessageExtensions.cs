// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation {
    internal static class PlotMessageExtensions {
        public static BitmapImage ToBitmapImage(this PlotMessage plot) {
            return BitmapImageFactory.Load(new MemoryStream(plot.Data));
        }
    }
}
