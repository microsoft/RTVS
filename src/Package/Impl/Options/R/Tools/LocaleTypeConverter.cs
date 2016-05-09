// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class LocaleTypeConverter : TypeConverter {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            var cultureList = new List<string>() { null };
            cultureList.AddRange(CultureExtensions.GetLanguageNames());
            return new StandardValuesCollection(cultureList);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            var s = value as string;
            if (s == Resources.Settings_DefaultValue) {
                return null;
            }
            return s;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            return value ?? Resources.Settings_DefaultValue;
        }
    }
}
