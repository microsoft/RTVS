// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class ReplShortcutTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(bool);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value.GetType() == typeof(bool))
                return value;

            if (value.GetType() == typeof(string)) {
                var s = value as string;
                if (s.EqualsIgnoreCase(Resources.CtrlEnter))
                    return true;

                if (s.EqualsIgnoreCase(Resources.CtrlECtrlE))
                    return false;
            }

            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string) || destinationType == typeof(bool);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value.GetType() == destinationType)
                return value;

            if (destinationType == typeof(string) && value.GetType() == typeof(bool)) {
                if ((bool)value)
                    return Resources.CtrlEnter;

                return Resources.CtrlECtrlE;
            } else if (destinationType == typeof(bool) && value.GetType() == typeof(string)) {
                return ConvertFrom(context, CultureInfo.CurrentUICulture, value);
            }

            return null;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            // only On/Off can be chosen
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(new bool[] { true, false });
        }
    }
}
