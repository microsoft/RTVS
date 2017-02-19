// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public abstract class EnumValueConverter<T> : IValueConverter {

        protected abstract IDictionary<T, string> GetEnumToString();
        protected abstract IDictionary<string, T> GetStringToEnum();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            List<string> names = new List<string>();
            if(targetType==typeof(IEnumerable) && value is IEnumerable) {
                foreach(T val in (IEnumerable)value) {
                    names.Add(ConvertEnumToString(val));
                }
                return names;
            }else if((targetType==typeof(string) || targetType == typeof(object)) && value is T) {
                return ConvertEnumToString((T)value);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(targetType == typeof(T) && value is string) {
                return ConvertStringToEnum(value as string);
            }
            return default(T);
        }

        private T ConvertStringToEnum(string key) {
            IDictionary<string, T> stringToEnumValues = GetStringToEnum();
            if(stringToEnumValues.ContainsKey(key)) {
                return stringToEnumValues[key];
            }
            return default(T);
        }

        private string ConvertEnumToString(T key) {
            IDictionary<T, string> enumToStringValues = GetEnumToString();
            if (enumToStringValues.ContainsKey(key)) {
                return enumToStringValues[key];
            }
            return string.Empty;
        }
    }
}
