// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Storage of the R application settings. Settings are written
    /// into R file as assignment statements such as 'name &lt;- value'.
    /// Value can be string or an expression. The difference is that
    /// string values are quoted when written into the file and expressions
    /// are written as is.
    /// </summary>
    public sealed class ConfigurationSettingsWriter : IConfigurationSettingsWriter {
        private StreamWriter _writer;

        public ConfigurationSettingsWriter(StreamWriter writer) {
            _writer = writer;
        }

        public void Dispose() {
            _writer?.Dispose();
            _writer = null;
        }

        /// <summary>
        /// Persists settings into R file. Settings are written
        /// into R file as assignment statements such as 'name &lt;- value'.
        /// Value
        /// </summary>
        public void SaveSettings(IEnumerable<IConfigurationSetting> settings) {
             WriteHeader();
            foreach (var s in settings) {
                var v = FormatValue(s);
                if (!string.IsNullOrWhiteSpace(v)) {
                    WriteAttributes(s);
                    _writer.WriteLine(Invariant($"{s.Name} <- {v}"));
                    _writer.WriteLine(string.Empty);
                }
            }
            _writer.Flush();
        }

        private void WriteHeader() {
            _writer.WriteLine("# Application settings file.");
            _writer.WriteLine(string.Format(CultureInfo.CurrentCulture, "# File content was generated on {0}", DateTime.Now));
            _writer.WriteLine(string.Empty);
        }

        private void WriteAttributes(IConfigurationSetting s) {
            foreach (var attribute in s.Attributes) {
                var value = attribute.Value;
                if (!string.IsNullOrEmpty(value)) {
                    _writer.WriteLine(Invariant($"# {ConfigurationSettingAttributeNames.GetPersistentKey(attribute.Name)} {value}"));
                }
            }
        }

        private static string FormatValue(IConfigurationSetting s) {
            if (s.ValueType == ConfigurationSettingValueType.String) {
                var hasSingleQuotes = s.Value.IndexOf('\'') >= 0;
                var hasDoubleQuotes = s.Value.IndexOf('\"') >= 0;
                if (hasSingleQuotes && !hasDoubleQuotes) {
                    return Invariant($"\"{s.Value}\"");
                } else if (!hasSingleQuotes) {
                    return Invariant($"'{s.Value}'");
                }
                // TODO: Resources.ConfigurationError_Quotes; ?
            }
            return s.Value;
        }
    }
}
