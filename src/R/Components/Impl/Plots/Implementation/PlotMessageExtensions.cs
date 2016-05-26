// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Media.Imaging;
using Microsoft.R.Host.Client.Definitions;

namespace Microsoft.R.Components.Plots.Implementation {
    internal static class PlotMessageExtensions {
        public static BitmapImage ToBitmapImage(this PlotMessage plot) {
            // Use Begin/EndInit to avoid locking the file on disk
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(plot.FilePath);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            return bmp;
        }
    }
}
