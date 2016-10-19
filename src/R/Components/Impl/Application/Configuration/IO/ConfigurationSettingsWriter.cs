// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.R.Host.Client;
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
        /// into R file as assignment statements such as 'settings$name &lt;- value'.
        /// Value
        /// </summary>
        public void SaveSettings(IEnumerable<IConfigurationSetting> settings) {
            WriteHeader();
            _writer.WriteLine("settings <- as.environment(list())");
            _writer.WriteLine(string.Empty);
            foreach (var s in settings) {
                var v = FormatValue(s);
                if (!string.IsNullOrWhiteSpace(v)) {
                    WriteAttributes(s);
                    _writer.WriteLine(Invariant($"settings${s.Name} <- {v}"));
                    _writer.WriteLine(string.Empty);
                }
            }
            _writer.Flush();
        }

        private void WriteHeader() {
            _writer.WriteLine(Resources.SettingsFileHeader);
            _writer.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.SettingsFileGeneratedStamp, DateTime.Now));
            _writer.WriteLine(string.Empty);
        }

        private void WriteAttributes(IConfigurationSetting s) {
            if (!string.IsNullOrEmpty(s.Category)) {
                _writer.WriteLine(Invariant($"# [{ConfigurationSettingAttributeNames.Category}] {s.Category}"));
            }
            if (!string.IsNullOrEmpty(s.Description)) {
                _writer.WriteLine(Invariant($"# [{ConfigurationSettingAttributeNames.Description}] {s.Description}"));
            }
            if (!string.IsNullOrEmpty(s.EditorType)) {
                _writer.WriteLine(Invariant($"# [{ConfigurationSettingAttributeNames.Editor}] {s.EditorType}"));
            }
        }

        private static string FormatValue(IConfigurationSetting s) {
            if (s.ValueType == ConfigurationSettingValueType.String) {
                var hasSingleQuotes = s.Value.IndexOf('\'') >= 0;
                var hasDoubleQuotes = s.Value.IndexOf('\"') >= 0;
                if (hasSingleQuotes && !hasDoubleQuotes) {
                    return s.Value.ToRStringLiteral(quote: '"');
                } else {
                    return s.Value.ToRStringLiteral(quote: '\'');
                }
            }
            return s.Value;
        }
    }
}
