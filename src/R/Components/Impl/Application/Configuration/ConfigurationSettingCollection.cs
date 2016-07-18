// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
                try {
                    Clear();
                    using (var sr = new StreamReader(filePath)) {
                        using (var csr = new ConfigurationSettingsReader(sr)) {
                            var settings = csr.LoadSettings();
                            foreach (var s in settings) {
                                Add(s);
                            }
                        }
                    }
                    SourceFile = filePath;
                } catch(IOException) {
                    SourceFile = null;
                } catch(UnauthorizedAccessException) {
                    SourceFile = null;
                }
            }
        }

        /// <summary>
        /// Writes settings to a disk file.
        /// </summary>
        public void Save(string filePath = null) {
            lock (_lock) {
                SourceFile = filePath ?? SourceFile;
                if(SourceFile == null) {
                    throw new InvalidOperationException("Either settings must have been previously loaded from the existing file or a file name must be provided");
                }
                if (Count > 0) {
                    using (var sw = new StreamWriter(SourceFile)) {
                        using (var csw = new ConfigurationSettingsWriter(sw)) {
                            csw.SaveSettings(this);
                        }
                    }
                }
            }
        }

        public string SourceFile { get; private set; }
    }
}
