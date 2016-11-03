// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.Settings;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    internal sealed class TestSettingsStore : WritableSettingsStore {
        private readonly Dictionary<string, Dictionary<string, object>> _collections = new Dictionary<string, Dictionary<string, object>>();

        public override bool CollectionExists(string collectionPath) {
            return _collections.ContainsKey(collectionPath);
        }

        public override void CreateCollection(string collectionPath) {
            if (CollectionExists(collectionPath)) {
                throw new InvalidOperationException(collectionPath);
            }
            _collections[collectionPath] = new Dictionary<string, object>();
        }

        public override bool DeleteCollection(string collectionPath) {
            if (CollectionExists(collectionPath)) {
                _collections.Remove(collectionPath);
                return true;
            }
            return false;
        }

        public override bool DeleteProperty(string collectionPath, string propertyName) {
            _collections[collectionPath].Remove(propertyName);
            return true;
        }

        public override bool GetBoolean(string collectionPath, string propertyName) => GetValue<bool>(collectionPath, propertyName);
        public override bool GetBoolean(string collectionPath, string propertyName, bool defaultValue) => GetValue<bool>(collectionPath, propertyName, defaultValue);
        public override int GetInt32(string collectionPath, string propertyName) => GetValue<int>(collectionPath, propertyName);
        public override int GetInt32(string collectionPath, string propertyName, int defaultValue) => GetValue<int>(collectionPath, propertyName, defaultValue);
        public override long GetInt64(string collectionPath, string propertyName) => GetValue<long>(collectionPath, propertyName);
        public override long GetInt64(string collectionPath, string propertyName, long defaultValue) => GetValue<long>(collectionPath, propertyName, defaultValue);
        public override DateTime GetLastWriteTime(string collectionPath) => DateTime.Now;
        public override MemoryStream GetMemoryStream(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override int GetPropertyCount(string collectionPath) => _collections[collectionPath].Keys.Count;
        public override IEnumerable<string> GetPropertyNames(string collectionPath) => _collections[collectionPath].Keys;

        public override SettingsType GetPropertyType(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override string GetString(string collectionPath, string propertyName) => GetValue<string>(collectionPath, propertyName);
        public override string GetString(string collectionPath, string propertyName, string defaultValue)=> GetValue<string>(collectionPath, propertyName, defaultValue);

        public override int GetSubCollectionCount(string collectionPath) {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetSubCollectionNames(string collectionPath) {
            throw new NotImplementedException();
        }

        public override uint GetUInt32(string collectionPath, string propertyName) => GetValue<uint>(collectionPath, propertyName);
        public override uint GetUInt32(string collectionPath, string propertyName, uint defaultValue) => GetValue<uint>(collectionPath, propertyName, defaultValue);
        public override ulong GetUInt64(string collectionPath, string propertyName) => GetValue<ulong>(collectionPath, propertyName);
        public override ulong GetUInt64(string collectionPath, string propertyName, ulong defaultValue) => GetValue<ulong>(collectionPath, propertyName, defaultValue);
        public override bool PropertyExists(string collectionPath, string propertyName) => _collections[collectionPath].ContainsKey(propertyName);

        public override void SetBoolean(string collectionPath, string propertyName, bool value) => _collections[collectionPath][propertyName] = value;
        public override void SetInt32(string collectionPath, string propertyName, int value) => _collections[collectionPath][propertyName] = value;
        public override void SetInt64(string collectionPath, string propertyName, long value) => _collections[collectionPath][propertyName] = value;
        public override void SetMemoryStream(string collectionPath, string propertyName, MemoryStream value) {
            throw new NotImplementedException();
        }
        public override void SetString(string collectionPath, string propertyName, string value) => _collections[collectionPath][propertyName] = value;
        public override void SetUInt32(string collectionPath, string propertyName, uint value) => _collections[collectionPath][propertyName] = value;
        public override void SetUInt64(string collectionPath, string propertyName, ulong value) => _collections[collectionPath][propertyName] = value;

        private T GetValue<T>(string collectionPath, string propertyName) {
            object value;
            _collections[collectionPath].TryGetValue(propertyName, out value);
            return (T)value;
        }

        private T GetValue<T>(string collectionPath, string propertyName, T defaultValue) {
            if (_collections[collectionPath].ContainsKey(propertyName)) {
                return GetValue<T>(collectionPath, propertyName);
            }
            return defaultValue;
        }
    }
}
