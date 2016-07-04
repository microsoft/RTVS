// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSettingCollection : 
        ObservableCollection<IConfigurationSetting>, 
        IConfigurationSettingCollection {
        private readonly object _lock = new object();

        /// <summary>
        /// Retrieves existing setting. Returns null if setting does not exist
        /// </summary>
        public IConfigurationSetting GetSetting(string settingName) {
            return this.FirstOrDefault(x => x.Name.EqualsOrdinal(settingName));
        }

        /// <summary>
        /// Loads settings from the file
        /// </summary>
        public void Load(string filePath) {
            lock (_lock) {
                using (var sr = new StreamReader(filePath)) {
                    using (var csr = new ConfigurationSettingsReader(sr, new ConfigurationSettingAttributeFactoryProvider())) {
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
