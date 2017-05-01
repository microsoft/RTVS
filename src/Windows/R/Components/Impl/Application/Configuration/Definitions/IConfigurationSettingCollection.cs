// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.R.Components.Application.Configuration {
    public interface IConfigurationSettingCollection: 
        ICollection<IConfigurationSetting>,
        INotifyCollectionChanged, INotifyPropertyChanged {
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
        void Save(string filePath = null);

        /// <summary>
        /// Path to the settings file on disk
        /// </summary>
        string SourceFile { get; }
    }
}
