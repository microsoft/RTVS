// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class EnumTypeConverter<T> : TypeConverter {
        private readonly List<T> _enumValues;
        private readonly List<string> _displayNames;

        protected EnumTypeConverter(params string[] localizedDisplayNames)
            : this(0, localizedDisplayNames) {
        }

        protected EnumTypeConverter(int startIndex, string[] localizedDisplayNames) {
            Array values = Enum.GetValues(typeof(T));
            int nameCount = localizedDisplayNames.Length;

            if (startIndex < 0 || startIndex >= values.Length) {
                throw new ArgumentException("Start index is out of range", nameof(startIndex));
            }

            if (startIndex + nameCount > values.Length) {
                throw new ArgumentException("Wrong number of localized display names", nameof(localizedDisplayNames));
            }

            _enumValues = new List<T>(nameCount);
            _displayNames = new List<string>(nameCount);

            for (int i = 0; i < nameCount; i++) {
                _enumValues.Add((T)values.GetValue(i + startIndex));
                _displayNames.Add(localizedDisplayNames[i + startIndex]);
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(T);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value != null && value.GetType() == typeof(T)) {
                return value;
            }

            string valueName = value as string;
            if (valueName != null) {
                for (var i = 0; i < _displayNames.Count; i++) {
                    if (_displayNames[i].Equals(valueName)) {
                        return _enumValues[i];
                    }
                }
            }

            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(T) || destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value?.GetType() == destinationType) {
                return value;
            }

            if (destinationType == typeof(string) && value.GetType() == typeof(T)) {
                T enumValue = (T)value;

                for (int i = 0; i < _enumValues.Count; i++) {
                    if (_enumValues[i].Equals(enumValue)) {
                        return _displayNames[i];
                    }
                }

                // Have to return something to avoid an ArgumentNull exception
                return string.Empty;
            }

            return null;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            // Only the enum values can be chosen
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(_enumValues);
        }
    }
}
