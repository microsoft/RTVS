// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.Settings;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    public sealed class TestSettingsStore : WritableSettingsStore {
        public override bool CollectionExists(string collectionPath) {
            throw new NotImplementedException();
        }

        public override void CreateCollection(string collectionPath) {
            throw new NotImplementedException();
        }

        public override bool DeleteCollection(string collectionPath) {
            throw new NotImplementedException();
        }

        public override bool DeleteProperty(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override bool GetBoolean(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override bool GetBoolean(string collectionPath, string propertyName, bool defaultValue) {
            throw new NotImplementedException();
        }

        public override int GetInt32(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override int GetInt32(string collectionPath, string propertyName, int defaultValue) {
            throw new NotImplementedException();
        }

        public override long GetInt64(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override long GetInt64(string collectionPath, string propertyName, long defaultValue) {
            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTime(string collectionPath) {
            throw new NotImplementedException();
        }

        public override MemoryStream GetMemoryStream(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override int GetPropertyCount(string collectionPath) {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetPropertyNames(string collectionPath) {
            throw new NotImplementedException();
        }

        public override SettingsType GetPropertyType(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override string GetString(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override string GetString(string collectionPath, string propertyName, string defaultValue) {
            throw new NotImplementedException();
        }

        public override int GetSubCollectionCount(string collectionPath) {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetSubCollectionNames(string collectionPath) {
            throw new NotImplementedException();
        }

        public override uint GetUInt32(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override uint GetUInt32(string collectionPath, string propertyName, uint defaultValue) {
            throw new NotImplementedException();
        }

        public override ulong GetUInt64(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override ulong GetUInt64(string collectionPath, string propertyName, ulong defaultValue) {
            throw new NotImplementedException();
        }

        public override bool PropertyExists(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override void SetBoolean(string collectionPath, string propertyName, bool value) {
            throw new NotImplementedException();
        }

        public override void SetInt32(string collectionPath, string propertyName, int value) {
            throw new NotImplementedException();
        }

        public override void SetInt64(string collectionPath, string propertyName, long value) {
            throw new NotImplementedException();
        }

        public override void SetMemoryStream(string collectionPath, string propertyName, MemoryStream value) {
            throw new NotImplementedException();
        }

        public override void SetString(string collectionPath, string propertyName, string value) {
            throw new NotImplementedException();
        }

        public override void SetUInt32(string collectionPath, string propertyName, uint value) {
            throw new NotImplementedException();
        }

        public override void SetUInt64(string collectionPath, string propertyName, ulong value) {
            throw new NotImplementedException();
        }
    }
}
