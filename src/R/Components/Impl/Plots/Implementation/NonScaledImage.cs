// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots.Implementation {
    /// <summary>
    /// A bitmap image that is rendered to the screen without being scaled,
    /// where each pixel in the bitmap takes one physical pixel on screen.
    /// </summary>
    internal class NonScaledImage : Image {
        protected override Size MeasureOverride(Size constraint) {
            BitmapImage bmp = this.Source as BitmapImage;
            if (bmp != null) {
                // WPF assumes that your code doesn't have special dpi handling
                // and automatically sizes bitmaps to avoid them being too
                // small when running at high dpi.
                // We prevent that scaling by calculating a size based on
                // pixel size and dpi setting.
                Size bitmapSize = new Size(bmp.PixelWidth, bmp.PixelHeight);
                return WpfUnitsConversion.FromPixels(PresentationSource.FromVisual(this), bitmapSize);
            }
            return base.MeasureOverride(constraint);
        }
    }
}
