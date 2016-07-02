// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Components.Application.Configuration {
    [Export(typeof(IConfigurationSettingsService))]
    public sealed class ConfigurationSettingsService : IConfigurationSettingsService {
        private readonly ConfigurationSettingCollection _settings = new ConfigurationSettingCollection();
        private readonly ICoreShell _coreShell;

        internal IFileSystem FileSystem { get; set; } = new FileSystem();

        [ImportingConstructor]
        public ConfigurationSettingsService(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        /// <summary>
        /// Complete collection of settings
        /// </summary>
        public ConfigurationSettingCollection Settings => _settings;

        /// <summary>
        /// Retrieves existing setting. Returns null if setting does not exist
        /// </summary>
        public IConfigurationSetting GetSetting(string settingName) {
            return _settings.FirstOrDefault(x => x.Name.EqualsOrdinal(settingName));
        }

        public void Save(string filePath) {
            _settings.Save(filePath);
        }

        public void Load(string filePath) {
            _settings.Load(filePath);
        }
    }
}