// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class BooleanToVisibilityConverter : IValueConverter {
        public BooleanToVisibilityConverter() {
            ValueForTrue = Visibility.Visible;
            ValueForFalse = Visibility.Collapsed;
        }

        public Visibility ValueForFalse { get; set; }
        public Visibility ValueForTrue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool)value) ? ValueForTrue : ValueForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

    }
}
