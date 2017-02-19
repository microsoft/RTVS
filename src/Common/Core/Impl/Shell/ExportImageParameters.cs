// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core {
    public class ExportImageParameters {
        public string FilePath { get; set; }
        public int PixelWidth { get; set;  }
        public int PixelHeight { get; set; }
        public int Resolution { get; set; }
        public bool ViewPlot { get; set; }
    }
}
