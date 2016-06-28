// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Collection of the R application settings.
    /// Presented in the project properties.
    /// </summary>
    public sealed class ConfigurationSettingsCollection: ICollection<IConfigurationSetting> {
        private readonly Dictionary<string, IConfigurationSetting> _settings = new Dictionary<string, IConfigurationSetting>();

        public int Count => _settings.Keys.Count;
        public bool IsReadOnly => false;
        public void Add(IConfigurationSetting item)  =>_settings[item.Name] = item;
        public void Clear() => _settings.Clear();
        public bool Contains(IConfigurationSetting item) => _settings.ContainsKey(item.Name);
        public bool Remove(IConfigurationSetting item) => _settings.Remove(item.Name);

        public void CopyTo(IConfigurationSetting[] array, int arrayIndex) {
            _settings.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IConfigurationSetting> GetEnumerator() => _settings.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>_settings.Values.GetEnumerator();
    }
}
