// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    internal sealed class VsSettingsStorage : ISettingsStorage, IDisposable {
        /// <summary>
        /// Settings cache. Persisted to storage when package is unloaded.
        /// </summary>
        private readonly Dictionary<string, Setting> _settingsCache = new Dictionary<string, Setting>();
        private readonly object _lock = new object();

        private ISettingsManager _settingsManager;
        private ISettingsSubset _subset;

        public VsSettingsStorage() : this(null) { }
        public VsSettingsStorage(ISettingsManager settingsManager) {
            _settingsManager = settingsManager;
        }

        private ISettingsManager SettingsManager {
            get {
                if (_settingsManager == null) {
                    _settingsManager = RPackage.GetGlobalService(typeof(SVsSettingsPersistenceManager)) as ISettingsManager;
                    _subset = _settingsManager.GetSubset(RPackage.ProductName + "*");
                    _subset.SettingChangedAsync += OnSettingChangedAsync;
                }
                return _settingsManager;
            }
        }

        private Task OnSettingChangedAsync(object sender, PropertyChangedEventArgs args) {
            // TODO: update connections and cache dynamically
            return Task.CompletedTask;
        }

        public void Dispose() {
            if (_subset != null) {
                _subset.SettingChangedAsync -= OnSettingChangedAsync;
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
                return result == GetValueResult.Success && value != null;
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
                    var newSetting = Setting.FromStoredValue(value, t);
                    _settingsCache[name] = newSetting;
                    return newSetting.Value;
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
                    _settingsCache[name] = new Setting(newValue, true);
                } else {
                    _settingsCache[name].SetValue(newValue);
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
                var setting = kvp.Value;
                await SettingsManager.SetValueAsync(persistentName, setting.ToSimpleType(), false);
                setting.Changed = false;
            }
        }
        #endregion

        internal void ClearCache() => _settingsCache.Clear(); // for tests

        private object GetValueFromStore(string name, Type t) {
            object value;
            var result = SettingsManager.TryGetValue(GetPersistentName(name), out value);
            return result == GetValueResult.Success ? value : null;
        }

        private static string GetPersistentName(string setting) => RPackage.ProductName + "." + setting;

        class Setting {
            public object Value { get; private set; }
            public bool Changed { get; set; }

            public Setting(object value, bool changed = false) {
                Value = value;
                Changed = changed;
            }

            public object ToSimpleType() {
                if (Value != null && !IsSimpleType(Value.GetType())) {
                    // Must escape JSON since VS roaming settings are converted to JSON
                    return JsonConvert.ToString(JsonConvert.SerializeObject(Value));
                }
                return Value;
            }

            public void SetValue(object newValue) {
                if (!Object.Equals(Value, newValue)) {
                    Changed = true;
                    Value = newValue;
                }
            }

            public static Setting FromStoredValue(object o, Type t) {
                object value = null;

                if (o is string && !IsSimpleType(t)) {
                    // Must unescape JSON since VS roaming settings are converted to JSON
                    var token = Json.ParseToken((string)o);
                    try {
                        var str = token.ToString();
                        value = Json.DeserializeObject(str, t);
                    } catch (ArgumentException) {
                        value = null; // Protect against stale or corrupted data in the roaming storage
                    }
                } else {
                    if (o is Int64 && t != typeof(Int64)) {
                        o = Convert.ToInt32(o);
                    }

                    // VS roaming setting manager roams integer values and enums as integers
                    if (o is Int32 && t.IsEnum && t.IsEnumDefined(o)) {
                        value = Enum.ToObject(t, o);
                    } else {
                        value = o;
                    }
                }
                return new Setting(value);
            }

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
}
