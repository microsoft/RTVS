// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace Microsoft.R.Wpf.ValueConverters {
    public class EnumDescriptionValueConverter : IValueConverter {
        public ResourceManager Resources { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return GetEnumDescription((Enum)value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        private string GetEnumDescription(Enum enumValue, CultureInfo culture) {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var attribArray = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attribArray.Length == 0) {
                return enumValue.ToString();
            }

            var attrib = attribArray[0] as DescriptionAttribute;
            if (string.IsNullOrEmpty(attrib?.Description)) {
                return enumValue.ToString();
            }

            var resourceString = Resources.GetString(attrib.Description, culture);
            return !string.IsNullOrEmpty(resourceString) ? resourceString : attrib.Description;
        }
    }
}
