// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public class PDFPaperSizeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value!=null && ((PDFExportOptions)value).PaperName == Resources.Combobox_Custom) {
                return true;
            }
            return false;
        }       
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
}
