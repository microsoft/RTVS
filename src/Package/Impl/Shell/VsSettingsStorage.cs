// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.Shell;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Settings;
using Newtonsoft.Json;

namespace Microsoft.Internal.VisualStudio.Shell.Interop {
    // The definition is not public for whatever reason
    [Guid("9B164E40-C3A2-4363-9BC5-EB4039DEF653")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface SVsSettingsPersistenceManager { }
}

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Represents VS user settings collection. 
    /// Provides methods for saving values in VS settings.
    /// </summary>
    [Export(typeof(ISettingsStorage))]
    internal sealed class VsSettingsStorage : ISettingsStorage {
        class Setting {
            public object Value;
            public bool Changed;

            public Setting(object value) {
                Value = value;
            }
        }

        /// <summary>
        /// Settings cache. Persisted to storage when package is unloaded.
        /// </summary>
        private readonly Dictionary<string, Setting> _settingsCache = new Dictionary<string, Setting>();
        private readonly object _lock = new object();

        private ISettingsManager _settingsManager;
        private ISettingsManager SettingsManager {
            get {
                if (_settingsManager == null) {
                    _settingsManager = RPackage.GetGlobalService(typeof(SVsSettingsPersistenceManager)) as ISettingsManager;
                }
                return _settingsManager;
            }
        }

        #region ISettingsStorage
        public bool SettingExists(string name) {
            lock (_lock) {
                if (_settingsCache.ContainsKey(name)) {
                    return true;
                }

                object value;
                var result = SettingsManager.TryGetValue(GetPersistentName(name), out value);
                return result == GetValueResult.Success;
            }
        }

        public object GetSetting(string name, Type t) {
            lock (_lock) {
                Setting setting;
                if (_settingsCache.TryGetValue(name, out setting)) {
                    return setting.Value;
                }

                var value = GetValueFromStore(name, t);
                if (value != null) {
                    _settingsCache[name] = new Setting(value);
                    return value;
                }
            }
            return null;
        }

        public T GetSetting<T>(string name, T defaultValue) {
            object value = GetSetting(name, typeof(T));
            if (value != null) {
                Debug.Assert(value is T);
                return (T)value;
            }
            return defaultValue;
        }

        public void SetSetting(string name, object newValue) {
            lock (_lock) {
                if (!_settingsCache.ContainsKey(name)) {
                    _settingsCache[name] = new Setting(newValue);
                } else {
                    if (!Object.Equals(_settingsCache[name].Value, newValue)) {
                        _settingsCache[name].Changed = true;
                        _settingsCache[name].Value = newValue;
                    }
                }
            }
        }

        public async Task PersistAsync() {
            KeyValuePair<string, Setting>[] kvps;

            lock (_lock) {
                kvps = _settingsCache.Where(k => k.Value.Changed).ToArray();
            }

            foreach (var kvp in kvps) {
                var persistentName = GetPersistentName(kvp.Key);
                object value = null;
                var setting = kvp.Value;
                if (setting.Value != null) {
                    value = setting.Value;
                    if (!IsSimpleType(value.GetType())) {
                        // Must escape JSON since VS roaming settings are converted to JSON
                        value = JsonConvert.ToString(JsonConvert.SerializeObject(value));
                    }
                }
                await SettingsManager.SetValueAsync(persistentName, value, false);
                setting.Changed = false;
            }
        }
        #endregion

        internal void ClearCache() => _settingsCache.Clear(); // for tests

        private object GetValueFromStore(string name, Type t) {
            object value;
            var result = SettingsManager.TryGetValue(GetPersistentName(name), out value);
            if (value is string && !IsSimpleType(t)) {
                // Must unescape JSON since VS roaming settings are converted to JSON
                var token = Json.ParseToken((string)value);
                try {
                    var str = token.ToObject<string>();
                    value = Json.DeserializeObject(str, t);
                } catch (ArgumentException) {
                    value = null; // Protect against stale or corrupted data in the roaming storage
                }
            } else if ((value is Int64 && t != typeof(Int64)) || (value is Int32 && t != typeof(Int32))) {
                // VS roaming setting manager roams integer values and enums as integers
                value = Convert.ToInt32(value);
                if (t.IsEnum && t.IsEnumDefined(value)) {
                    value = Enum.ToObject(t, value);
                }
            }
            return result == GetValueResult.Success ? value : null;
        }

        private static string GetPersistentName(string setting) => RPackage.ProductName + "." + setting;
        private static bool IsSimpleType(Type t) {
            return t.IsEnum ||
                   t == typeof(string) ||
                   t == typeof(bool) ||
                   t == typeof(int) ||
                   t == typeof(uint) ||
                   t == typeof(double) ||
                   t == typeof(float);
        }
    }
}
