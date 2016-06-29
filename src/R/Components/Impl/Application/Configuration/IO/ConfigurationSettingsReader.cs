// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.R.Components.Application.Configuration.Parser;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Storage of the R application settings. Settings are written
    /// into R file as assignment statements such as 'name &lt;- value'.
    /// Value can be string or an expression. The difference is that
    /// string values are quoted when written into the file and expressions
    /// are written as is.
    /// </summary>
    public sealed class ConfigurationSettingsReader : IConfigurationSettingsReader {
        private readonly Stream _stream;
        public ConfigurationSettingsReader(Stream stream) {
            _stream = stream;
        }

        public void Dispose() {
            _stream?.Dispose();
        }

        public IReadOnlyList<IConfigurationSetting> LoadSettings() {
            var settings = new List<IConfigurationSetting>();
            var sr = new StreamReader(_stream);
            var cp = new ConfigurationParser(sr);
            while (true) {
                var s = cp.ReadSetting();
                if (s == null) {
                    break;
                }
                settings.Add(s);
            }
            return settings;
        }
    }
}
