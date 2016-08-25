// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public struct PlotDeviceProperties {
        public int Width { get; }
        public int Height { get; }
        public int Resolution { get; }

        public PlotDeviceProperties(int width, int height, int resolution) {
            Width = width;
            Height = height;
            Resolution = resolution;
        }

        public static PlotDeviceProperties Default { get; } = new PlotDeviceProperties(360, 360, 96);
    }
}
