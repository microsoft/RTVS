// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace Microsoft.R.Components.Plots.Implementation {
    internal static class WpfUnitsConversion {
        public static Size FromPixels(Visual visual, Size pixelSize) {
            var source = PresentationSource.FromVisual(visual);
            return (Size)source.CompositionTarget.TransformFromDevice.Transform((Vector)pixelSize);
        }

        public static Size ToPixels(Visual visual, Size wpfSize) {
            var source = PresentationSource.FromVisual(visual);
            return (Size)source.CompositionTarget.TransformToDevice.Transform((Vector)wpfSize);
        }

        public static Point ToPixels(Visual visual, Point wpfSize) {
            var source = PresentationSource.FromVisual(visual);
            return (Point)source.CompositionTarget.TransformToDevice.Transform((Vector)wpfSize);
        }

        public static int GetResolution(Visual visual) {
            var source = PresentationSource.FromVisual(visual);
            int res = (int)(96 * source.CompositionTarget.TransformToDevice.M11);
            return res;
        }
    }
}
