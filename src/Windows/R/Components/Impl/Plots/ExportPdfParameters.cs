// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots {
    public class ExportPdfParameters {
        public const string RInternalPaperName_Special = "special";
        public const string RInternalPaperName_A4 = "a4";
        public const string RInternalPaperName_US = "us";
        public const string RInternalPaperName_Legal = "legal";
        public const string RInternalPaperName_Executive = "executive";
        public const string RInternalPaperName_A4Rotated = "a4r";
        public const string RInternalPaperName_USRotated = "USr";

        public const string RInternalPdfDevice_PDF = "pdf";
        public const string RInternalPdfDevice_CairoPDF = "cairo_pdf";

        public ExportPdfParameters() {
            RInternalPaperName = RInternalPaperName_Special;
            RInternalPdfDevice = RInternalPdfDevice_PDF;
        }

        public ExportPdfParameters(PDFExportOptions options, string orientation, string pdfPrint, bool viewPlot) {
            if (orientation == Resources.Combobox_Portrait) {
                if (options.PaperName == Resources.Combobox_A4) {
                    RInternalPaperName = RInternalPaperName_A4;
                } else if (options.PaperName == Resources.Combobox_UsLetter) {
                    RInternalPaperName = RInternalPaperName_US;
                } else if (options.PaperName == Resources.Combobox_UsLegal) {
                    RInternalPaperName = RInternalPaperName_Legal;
                } else if (options.PaperName == Resources.Combobox_USExecutive) {
                    RInternalPaperName = RInternalPaperName_Executive;
                } else {
                    RInternalPaperName = RInternalPaperName_Special;
                }
                WidthInInches = options.PaperWidth;
                HeightInInches = options.PaperHeight;
            } else if (orientation == Resources.Combobox_Landscape) {
                if (options.PaperName == Resources.Combobox_A4) {
                    RInternalPaperName = RInternalPaperName_A4Rotated;
                } else if (options.PaperName == Resources.Combobox_UsLetter) {
                    RInternalPaperName = RInternalPaperName_USRotated;
                } else {
                    RInternalPaperName = RInternalPaperName_Special;
                }
                HeightInInches = options.PaperWidth;
                WidthInInches = options.PaperHeight;
            }

            if(pdfPrint == Resources.Combobox_CairoPdf) {
                RInternalPdfDevice = RInternalPdfDevice_CairoPDF;
            } else {
                RInternalPdfDevice = RInternalPdfDevice_PDF;
            }
            ViewPlot = viewPlot;
        }

        public string FilePath { get; set; }
        public double WidthInInches { get; set; }
        public double HeightInInches { get; set; }
        public string RInternalPaperName { get; }
        public string RInternalPdfDevice { get; set; }
        public int Resolution { get; set; }
        public bool ViewPlot { get; set; }
    }
}
