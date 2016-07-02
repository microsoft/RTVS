// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.IO;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSettingCollection : ObservableCollection<IConfigurationSetting> {
        private readonly object _lock = new object();

        /// <summary>
        /// Loads settings from the file
        /// </summary>
        public void Load(string filePath) {
            lock (_lock) {
                using (var sr = new StreamReader(filePath)) {
                    using (var csr = new ConfigurationSettingsReader(sr)) {
                        var settings = csr.LoadSettings();
                        foreach(var s in settings) {
                            Add(s);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes settings to a disk file.
        /// </summary>
        public void Save(string filePath) {
            lock (_lock) {
                if (Count > 0) {
                    using (var sw = new StreamWriter(filePath)) {
                        using (var csw = new ConfigurationSettingsWriter(sw)) {
                            csw.SaveSettings(this);
                        }
                    }
                }
            }
        }
    }
}
