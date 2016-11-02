// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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

        /// <summary>
        /// VS internal settings manager <seealso cref="IVsSettingsManager"/>
        /// </summary>
        private readonly SettingsManager _settingsManager;

        /// <summary>
        /// Settings store provided by the <see cref="ShellSettingsManager"/>
        /// </summary>
        private readonly WritableSettingsStore _store;
        private readonly object _lock = new object();

        public VsSettingsStorage(SettingsManager sm) {
            _settingsManager = sm;
            _store = _settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            Debug.Assert(_store != null);
            EnsureCollectionExists();
        }

        public bool SettingExists(string name) {
            lock (_lock) {
                if (_settingsCache.ContainsKey(name)) {
                    return true;
                }
                return _store.PropertyExists(_collectionPath, name);
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
                        _store.SetBoolean(_collectionPath, s.Key, (bool)s.Value);
                    } else if (s.Value is int || t.IsEnum) {
                        _store.SetInt32(_collectionPath, s.Key, (int)s.Value);
                    } else if (s.Value is uint) {
                        _store.SetUInt32(_collectionPath, s.Key, (uint)s.Value);
                    } else if (s.Value is string) {
                        _store.SetString(_collectionPath, s.Key, (string)s.Value);
                    } else {
                        var json = JsonConvert.SerializeObject(s.Value);
                        _store.SetString(_collectionPath, s.Key, json);
                    }
                }
            }
        }

        private object GetValueFromStore(string name, Type t) {
            if (_store.CollectionExists(_collectionPath) && _store.PropertyExists(_collectionPath, name)) {
                if (typeof(bool).IsAssignableFrom(t)) {
                    return _store.GetBoolean(_collectionPath, name);
                } else if (typeof(int).IsAssignableFrom(t) || t.IsEnum) {
                    return _store.GetInt32(_collectionPath, name);
                } else if (typeof(uint).IsAssignableFrom(t)) {
                    return _store.GetUInt32(_collectionPath, name);
                } else if (typeof(string).IsAssignableFrom(t)) {
                    return _store.GetString(_collectionPath, name);
                } else {
                    var s = _store.GetString(_collectionPath, name);
                    if (s != null) {
                        return JsonConvert.DeserializeObject(s);
                    }
                }
            }
            return null;
        }

        private void SaveStringCollectionToStore(string name, IEnumerable<string> values) {
            string subCollectionPath = Invariant($"{_collectionPath}/{name}");
            _store.CreateCollection(subCollectionPath);

            int i = 0;
            foreach (var v in values) {
                _store.SetString(subCollectionPath, Invariant($"Value{i}"), v);
                i++;
            }
        }

        private void EnsureCollectionExists() {
            if (!_store.CollectionExists(_collectionPath)) {
                _store.CreateCollection(_collectionPath);
            }
        }
    }
}
