// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Components.Plots {
    public class PDFExportOptions {

            public PDFExportOptions(string paperName, double paperWidth, double paperHeight) {
                PaperName = paperName;
                PaperWidth = paperWidth;
                PaperHeight = paperHeight;
            }
            public string PaperName { get; }
            public double PaperWidth { get; set; }
            public double PaperHeight { get; set; }

        public static IEnumerable<PDFExportOptions> GetPdfPapers(double pixelWidth, double pixelHeight,int resolution) {
            double width = PixelsToInches(pixelWidth, resolution);
            double height = PixelsToInches(pixelHeight, resolution);
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

        public static IEnumerable<string> GetPaperOrientations() {
            IList<string> paperOrientations = new List<string>();
            paperOrientations.Add(Resources.Combobox_Portrait);
            paperOrientations.Add(Resources.Combobox_Landscape);
            return paperOrientations;
        }

        public static IEnumerable<string> GetPdfDeviceOptions() {
            IList<string> pdfPrintOptions = new List<string>();
            pdfPrintOptions.Add(Resources.Combobox_DefaultPdf);
            pdfPrintOptions.Add(Resources.Combobox_CairoPdf);
            return pdfPrintOptions;
        }

        private static double PixelsToInches(double pixels,int resolution) {
            return Math.Round(pixels / resolution, 2);
        }
    }
}
