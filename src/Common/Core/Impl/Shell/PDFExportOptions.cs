// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Common.Core {
    public class PDFExportOptions {
        public PDFExportOptions(string paperName, double paperWidth, double paperHeight) {
            PaperName = paperName;
            PaperWidth = paperWidth;
            PaperHeight = paperHeight;
        }

        public static List<PDFExportOptions> GetPdfPapers(double pixelWidth, double pixelHeight) {
            double width = PixelsToInches(pixelWidth);
            double height = PixelsToInches(pixelHeight);
            List<PDFExportOptions> pdfPapers = new List<PDFExportOptions>() {
                new PDFExportOptions(Resources.Combobox_DeviceSize, width, height),
                new PDFExportOptions(Resources.Combobox_UsLetter, 8.5, 11),
                new PDFExportOptions(Resources.Combobox_UsLegal, 8.5, 14),
                new PDFExportOptions(Resources.Combobox_USExecutive,7.25,10.25),
                new PDFExportOptions(Resources.Combobox_A4, 8.27, 11.69),
                new PDFExportOptions(Resources.Combobox_A5, 5.83, 8.27),
                new PDFExportOptions(Resources.Combobox_A6, 4.13, 5.83),
                new PDFExportOptions(Resources.Combobox_4x6inches, 4.0, 6.0),
                new PDFExportOptions(Resources.Combobox_5x7inches, 5.0, 7.0),
                new PDFExportOptions(Resources.Combobox_6x8inches, 6.0, 8.0),
                new PDFExportOptions(Resources.Combobox_Custom, width, height)
            };
            return pdfPapers;
        }

        public string PaperName { get; set; }
        public double PaperWidth { get; set; }
        public double PaperHeight { get; set; }

        public override string ToString() => PaperName;

        private static double PixelsToInches(double pixels) {
            return Math.Round(pixels / 96.0, 2);
        }
    }
}
