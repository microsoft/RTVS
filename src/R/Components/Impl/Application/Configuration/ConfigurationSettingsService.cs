// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;

namespace Microsoft.R.Components.Application.Configuration {
    [Export(typeof(IConfigurationSettingsService))]
    public sealed class ConfigurationSettingsService : IConfigurationSettingsService {
        private readonly IFileSystem _fileSystem;
        private readonly object _lock = new object();
        private List<IConfigurationSetting> _settings = new List<IConfigurationSetting>();

        public ConfigurationSettingsService(IFileSystem fs) {
            _fileSystem = fs;
        }

        /// <summary>
        /// Complete collection of settings
        /// </summary>
        public IReadOnlyList<IConfigurationSetting> Settings => _settings;

        public IConfigurationSetting GetSetting(string settingName) {
            lock (_lock) {
                return _settings.FirstOrDefault(x => x.Name.EqualsOrdinal(settingName));
            }
        }

        public IConfigurationSetting AddSetting(string name) {
            lock (_lock) {
                var setting = GetSetting(name);
                if (setting == null) {
                    setting = new ConfigurationSetting(name, null, ConfigurationSettingValueType.String);
                    _settings.Add(setting);
                }
                return setting;
            }
        }

        public void RemoveSetting(IConfigurationSetting s) {
            lock (_lock) {
                if (_settings.Contains(s)) {
                    _settings.Remove(s);
                }
            }
        }

        public void Save(string filePath) {
            lock (_lock) {
                if (_settings.Count > 0) {
                    using (var sw = new StreamWriter(filePath)) {
                        using (var csw = new ConfigurationSettingsWriter(sw)) {
                            csw.SaveSettings(_settings);
                        }
                    }
                }
            }
        }

        public void Load(string filePath) {
            lock (_lock) {
                using (var sr = new StreamReader(filePath)) {
                    using (var csr = new ConfigurationSettingsReader(sr)) {
                        _settings.AddRange(csr.LoadSettings());
                    }
                }
            }
        }
    }
}