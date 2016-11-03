// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Represents VS user settings collection. 
    /// Provides methods for saving values in VS settings.
    /// </summary>
    [Export(typeof(ISettingsStorage))]
    internal sealed class VsSettingsStorage : ISettingsStorage {
        /// <summary>
        /// Collection path in VS settings
        /// </summary>
        private const string _collectionPath = "R Tools";

        /// <summary>
        /// Settings cache. Persisted to storage when package is unloaded.
        /// </summary>
        private readonly Dictionary<string, object> _settingsCache = new Dictionary<string, object>();

        private readonly object _lock = new object();

        private SettingsManager _settingsManager;
        private SettingsManager SettingsManager {
            get {
                if (_settingsManager == null) {
                    _settingsManager = new ShellSettingsManager(RPackage.Current);
                }
                return _settingsManager;
            }
        }

        private WritableSettingsStore _store;
        private WritableSettingsStore Store {
            get {
                if (_store == null) {
                    _store = SettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
                    EnsureCollectionExists();
                }
                return _store;
            }
        }

        public VsSettingsStorage() : this(null) { }

        public VsSettingsStorage(SettingsManager sm) {
            _settingsManager = sm;
        }

        #region ISettingsStorage
        public bool SettingExists(string name) {
            lock (_lock) {
                if (_settingsCache.ContainsKey(name)) {
                    return true;
                }
                return Store.PropertyExists(_collectionPath, name);
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

        public void Persist() {
            lock (_lock) {
                EnsureCollectionExists();

                foreach (var s in _settingsCache) {
                    var t = s.Value.GetType();
                    if (s.Value is bool) {
                        Store.SetBoolean(_collectionPath, s.Key, (bool)s.Value);
                    } else if (s.Value is int || t.IsEnum) {
                        Store.SetInt32(_collectionPath, s.Key, (int)s.Value);
                    } else if (s.Value is uint) {
                        Store.SetUInt32(_collectionPath, s.Key, (uint)s.Value);
                    } else if (s.Value is string) {
                        Store.SetString(_collectionPath, s.Key, (string)s.Value);
                    } else {
                        var json = JsonConvert.SerializeObject(s.Value);
                        Store.SetString(_collectionPath, s.Key, json);
                    }
                }
            }
        }
        #endregion

        internal void ClearCache() => _settingsCache.Clear(); // for tests

        private object GetValueFromStore(string name, Type t) {
            if (Store.CollectionExists(_collectionPath) && Store.PropertyExists(_collectionPath, name)) {
                if (typeof(bool).IsAssignableFrom(t)) {
                    return Store.GetBoolean(_collectionPath, name);
                } else if (typeof(int).IsAssignableFrom(t) || t.IsEnum) {
                    return Store.GetInt32(_collectionPath, name);
                } else if (typeof(uint).IsAssignableFrom(t)) {
                    return Store.GetUInt32(_collectionPath, name);
                } else if (typeof(string).IsAssignableFrom(t)) {
                    return Store.GetString(_collectionPath, name);
                } else {
                    var s = Store.GetString(_collectionPath, name);
                    if (s != null) {
                        return JsonConvert.DeserializeObject(s, t);
                    }
                }
            }
            return null;
        }

        private void EnsureCollectionExists() {
            if (!Store.CollectionExists(_collectionPath)) {
                Store.CreateCollection(_collectionPath);
            }
        }
    }
}
