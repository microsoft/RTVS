// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.R.Components.Plots;

namespace Microsoft.R.Components.Test.Stubs {
    internal sealed class TestPlotExportDialog: IRPlotExportDialog {
        public string SaveFilePath { get; set; }

        public ExportImageParameters ShowExportImageDialog(ExportArguments imageArguments, string filter, string initialPath = null, string title = null)
            => new ExportImageParameters {
                FilePath = SaveFilePath,
                PixelWidth = 640,
                PixelHeight = 480,
                Resolution = 96,
                ViewPlot = false
            };

        public ExportPdfParameters ShowExportPdfDialog(ExportArguments pdfArguments, string filter, string initialPath = null, string title = null)
            => new ExportPdfParameters {
                FilePath = SaveFilePath,
                WidthInInches = 2,
                HeightInInches = 2,
                Resolution = 96,
                ViewPlot = false
            };
    }
}
