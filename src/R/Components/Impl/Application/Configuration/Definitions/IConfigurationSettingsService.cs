// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Application.Configuration {
    public interface IConfigurationSettingsService {
        /// <summary>
        /// Currently active collection of settings
        /// </summary>
        ConfigurationSettingCollection Settings { get; }

        /// <summary>
        /// Retrieves existing setting. Returns null if setting does not exist
        /// </summary>
        IConfigurationSetting GetSetting(string settingName);

        /// <summary>
        /// Loads settings from the file
        /// </summary>
        void Load(string filePath);

        /// <summary>
        /// Writes settings to a disk file.
        /// </summary>
        void Save(string filePath);
    }
}
