// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public class PrintOptionsEnumConverter : EnumValueConverter<PdfPrintOptionsEnum> {

        private static IDictionary<PdfPrintOptionsEnum, string> _enumToString = new Dictionary<PdfPrintOptionsEnum, string>() {
           {PdfPrintOptionsEnum.DefaultPDF, Resources.Combobox_DefaultPdf },
           {PdfPrintOptionsEnum.CairoPDF,Resources.Combobox_CairoPdf }
       };
        private static IDictionary<string, PdfPrintOptionsEnum> _stringToEnum = new Dictionary<string, PdfPrintOptionsEnum>() {
           { Resources.Combobox_DefaultPdf,PdfPrintOptionsEnum.DefaultPDF },
           {Resources.Combobox_CairoPdf ,PdfPrintOptionsEnum.CairoPDF}
       };

        protected override IDictionary<PdfPrintOptionsEnum, string> GetEnumToString() {
            return _enumToString;
        }

        protected override IDictionary<string, PdfPrintOptionsEnum> GetStringToEnum() {
            return _stringToEnum;
        }
    }
}
