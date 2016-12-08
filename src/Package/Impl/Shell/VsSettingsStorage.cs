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
        /// <summary>
        /// Settings cache. Persisted to storage when package is unloaded.
        /// </summary>
        private readonly Dictionary<string, object> _settingsCache = new Dictionary<string, object>();
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
                object value;
                if (_settingsCache.TryGetValue(name, out value)) {
                    return value;
                }

                value = GetValueFromStore(name, t);
                if (value != null) {
                    _settingsCache[name] = value;
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

        public void SetSetting(string name, object value) {
            lock (_lock) {
                _settingsCache[name] = value;
            }
        }

        public async Task PersistAsync() {
            KeyValuePair<string, object>[] kvps;

            lock (_lock) {
                kvps = _settingsCache.ToArray();
            }

            foreach (var kvp in kvps) {
                var persistentName = GetPersistentName(kvp.Key);
                if (kvp.Value != null) {
                    var t = kvp.Value.GetType();
                    object value = kvp.Value;
                    if (!IsSimpleType(t)) {
                        // Must escape JSON since VS roaming settings are converted to JSON
                        value = JsonConvert.ToString(JsonConvert.SerializeObject(value));
                    }
                    await SettingsManager.SetValueAsync(persistentName, value, false);
                } else {
                    await SettingsManager.SetValueAsync(persistentName, null, false);
                }
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
                var str = token.ToObject<string>();
                value = Json.DeserializeObject(str, t);
            } else if (value is Int64 && t != typeof(Int64)) {
                // VS roaming setting manager roams integer values and enums as Int64s
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
