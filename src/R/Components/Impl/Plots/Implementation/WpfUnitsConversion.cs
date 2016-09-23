// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.R.Components.Plots.Implementation {
    internal static class WpfUnitsConversion {
        public static Size FromPixels(PresentationSource source, Size pixelSize) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            return (Size)source.CompositionTarget.TransformFromDevice.Transform((Vector)pixelSize);
        }

        public static Size ToPixels(PresentationSource source, Size wpfSize) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            return (Size)source.CompositionTarget.TransformToDevice.Transform((Vector)wpfSize);
        }

        public static Point ToPixels(PresentationSource source, Point wpfSize) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            return (Point)source.CompositionTarget.TransformToDevice.Transform((Vector)wpfSize);
        }

        public static int GetResolution(PresentationSource source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            int res = (int)(96 * source.CompositionTarget.TransformToDevice.M11);
            return res;
        }
    }
}
