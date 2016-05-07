// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class LocaleTypeConverter : TypeConverter {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            var cultureList = new List<string>() { null };

            IEnumerable<CultureInfo> cultures = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
            IEnumerable<string> names = cultures.Select((c) => {
                var index = c.DisplayName.IndexOfAny(new char[] { ',', ' ' });
                return index >= 0 ? c.DisplayName.Substring(0, index).Trim() : c.DisplayName;
            })
            .Where(x => x.IndexOf(' ') < 0)
            .Distinct()
            .OrderBy(x => x);

            cultureList.AddRange(names);
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
