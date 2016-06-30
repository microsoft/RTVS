// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Components.Application.Configuration {
    [Export(typeof(IConfigurationSettingsService))]
    public sealed class ConfigurationSettingsService : IConfigurationSettingsService {
        private readonly IFileSystem _fileSystem;
        private readonly ICoreShell _coreShell;

        private List<IConfigurationSetting> _settings = new List<IConfigurationSetting>();
        private bool _fileChanged = true;
        private string _settingsFile = string.Empty;

        public ConfigurationSettingsService(IFileSystem fs) {
            _fileSystem = fs;
        }

        /// <summary>
        /// Full path to the current settings file
        /// </summary>
        public string ActiveSettingsFile {
            get { return _settingsFile; }
            set {
                if (!_settingsFile.EqualsIgnoreCase(value)) {
                    _settingsFile = value;
                    _fileChanged = true;
                }
            }
        }

        /// <summary>
        /// Complete collection of settings
        /// </summary>
        public IReadOnlyList<IConfigurationSetting> Settings => _settings;

        public IConfigurationSetting GetSetting(string settingName) {
            LoadSettingsFromFile();
            return _settings.FirstOrDefault(x => x.Name.EqualsOrdinal(settingName));
        }

        public IConfigurationSetting AddSetting(string name) {
            var setting = GetSetting(name);
            if (setting == null) {
                setting = new ConfigurationSetting(name, null, ConfigurationSettingValueType.String);
                _settings.Add(setting);
            }
            return setting;
        }

        public void RemoveSetting(IConfigurationSetting s) {
            if (_settings.Contains(s)) {
                _settings.Remove(s);
            }
        }

        public void Save() {
            if (ActiveSettingsFile != null) {
                using (var sw = new StreamWriter(ActiveSettingsFile)) {
                    using (var csw = new ConfigurationSettingsWriter(sw)) {
                        csw.SaveSettings(_settings);
                    }
                }
            }
        }

        private void LoadSettingsFromFile() {
            if (_fileChanged) {
                _fileChanged = false;
                _settings.Clear();

                if (_fileSystem.FileExists(ActiveSettingsFile)) {
                    using (var sr = new StreamReader(ActiveSettingsFile)) {
                        using (var csr = new ConfigurationSettingsReader(sr)) {
                            _settings.AddRange(csr.LoadSettings());
                        }
                    }
                } else {
                    throw new IOException(string.Format(CultureInfo.InvariantCulture, Resources.Error_SettingFileNoLongerExists, ActiveSettingsFile));
                }
            }
        }
    }
}