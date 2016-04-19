// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class CranMirrorTypeConverter : TypeConverter {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            var mirrors = new List<string> { null };
            mirrors.AddRange(CranMirrorList.MirrorNames);
            return new StandardValuesCollection(mirrors);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            string s = value as string;
            if (s == Resources.CranMirror_UseRProfile) {
                return null;
            } else if (CranMirrorList.MirrorNames.Contains(s)) {
                return s;
            } else {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnknownMirror, value));
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            return value ?? Resources.CranMirror_UseRProfile;
        }
    }
}
