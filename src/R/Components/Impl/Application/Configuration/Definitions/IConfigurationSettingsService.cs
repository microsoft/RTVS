// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Components.Application.Configuration {
    public interface IConfigurationSettingsService {
        /// <summary>
        /// Complete collection of settings
        /// </summary>
        IReadOnlyList<IConfigurationSetting> Settings { get; }

        /// <summary>
        /// Retrieves existing setting. Returns null if setting does not exist
        /// </summary>
        IConfigurationSetting GetSetting(string settingName);

        /// <summary>
        /// Creates new setting
        /// </summary>
        IConfigurationSetting AddSetting(string name);

        /// <summary>
        /// Removes existing setting
        /// </summary>
        void RemoveSetting(IConfigurationSetting s);

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
