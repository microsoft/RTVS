// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core {
    public class ExportPdfParameters {
        public ExportPdfParameters() {
            PaperName = "special";
        }

        public ExportPdfParameters(PDFExportOptions options, OrientationEnum orientation, PdfPrintOptionsEnum pdfPrint, bool viewPlot) {

            if (orientation == OrientationEnum.Portrait) {
                if (options.PaperName == Resources.Combobox_A4) {
                    PaperName = "a4";
                } else if (options.PaperName == Resources.Combobox_UsLetter) {
                    PaperName = "us";
                } else if (options.PaperName == Resources.Combobox_UsLegal) {
                    PaperName = "legal";
                } else if (options.PaperName == Resources.Combobox_USExecutive) {
                    PaperName = "executive";
                } else {
                    PaperName = "special";
                }
                WidthInInches = options.PaperWidth;
                HeightInInches = options.PaperHeight;
            } else if (orientation == OrientationEnum.Landscape) {
                if (options.PaperName == Resources.Combobox_A4) {
                    PaperName = "a4r";
                } else if (options.PaperName == Resources.Combobox_UsLetter) {
                    PaperName = "USr";
                } else {
                    PaperName = "special";
                }
                HeightInInches = options.PaperWidth;
                WidthInInches = options.PaperHeight;
            }
            PdfDevice = (pdfPrint == PdfPrintOptionsEnum.CairoPDF ? "cairo_pdf" : "pdf");
            ViewPlot = viewPlot;
        }

        public string FilePath { get; set; }
        public double WidthInInches { get; set; }
        public double HeightInInches { get; set; }
        public string PaperName { get; set; }
        public string PdfDevice { get; set; }
        public int Resolution { get; set; }
        public bool ViewPlot { get; set; }
    }
}
