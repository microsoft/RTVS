// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core {
    public class ExportArguments {
        public int PixelWidth { get; }
        public int PixelHeight { get; }
        public int Resolution { get; }

        public ExportArguments(int width, int height, int resolution) {
            PixelWidth = width;
            PixelHeight = height;
            Resolution = resolution;
        }
    }
}
