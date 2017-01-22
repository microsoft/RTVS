// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class LogLevelTypeConverter : TypeConverter {
        private static readonly string[] _permittedSettings = {
            Resources.LoggingLevel_None,
            Resources.LoggingLevel_Minimal,
            Resources.LoggingLevel_Normal,
            Resources.LoggingLevel_Traffic
        };
        private readonly int _maxLogLevel;

        public LogLevelTypeConverter() {
            var permissions = VsAppShell.Current.Services.LoggingPermissions;
            _maxLogLevel = (int)permissions.MaxVerbosity;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(_permittedSettings.Take(_maxLogLevel+1).ToList());
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(int);
        }

        /// <summary>
        /// Converts logging level name to the enumerated value
        /// </summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return Convert(value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string) || destinationType == typeof(int);
        }

        /// <summary>
        /// Converts logging level to a user-friendly display name
        /// </summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            return Convert(value);
        }

        private object Convert(object value) {
            if (value.GetType() == typeof(string)) {
                return ConvertToLevel(value as string);
            } else if (value.GetType() == typeof(int)) {
                return ConvertToName((int)value);
            }
            return null;
        }
        private int? ConvertToLevel(string name) {
            var index = _permittedSettings.IndexWhere(s => s.EqualsOrdinal(name));
            return index.Any() ? index.First() : (int?)null;
        }

        private string ConvertToName(int level) {
            return level >= 0 && level < _permittedSettings.Length ? _permittedSettings[level] : null;
        }
    }
}
