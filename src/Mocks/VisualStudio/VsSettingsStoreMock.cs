using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsSettingsStoreMock : IVsWritableSettingsStore {
        public int CollectionExists(string collectionPath, out int pfExists) {
            throw new NotImplementedException();
        }

        public int CreateCollection(string collectionPath) {
            throw new NotImplementedException();
        }

        public int DeleteCollection(string collectionPath) {
            throw new NotImplementedException();
        }

        public int DeleteProperty(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public int GetBinary(string collectionPath, string propertyName, uint byteLength, byte[] pBytes, uint[] actualByteLength) {
            throw new NotImplementedException();
        }

        public int GetBool(string collectionPath, string propertyName, out int value) {
            throw new NotImplementedException();
        }

        public int GetBoolOrDefault(string collectionPath, string propertyName, int defaultValue, out int value) {
            throw new NotImplementedException();
        }

        public int GetInt(string collectionPath, string propertyName, out int value) {
            throw new NotImplementedException();
        }

        public int GetInt64(string collectionPath, string propertyName, out long value) {
            throw new NotImplementedException();
        }

        public int GetInt64OrDefault(string collectionPath, string propertyName, long defaultValue, out long value) {
            throw new NotImplementedException();
        }

        public int GetIntOrDefault(string collectionPath, string propertyName, int defaultValue, out int value) {
            throw new NotImplementedException();
        }

        public int GetLastWriteTime(string collectionPath, SYSTEMTIME[] lastWriteTime) {
            throw new NotImplementedException();
        }

        public int GetPropertyCount(string collectionPath, out uint propertyCount) {
            throw new NotImplementedException();
        }

        public int GetPropertyName(string collectionPath, uint index, out string propertyName) {
            throw new NotImplementedException();
        }

        public int GetPropertyType(string collectionPath, string propertyName, out uint type) {
            throw new NotImplementedException();
        }

        public int GetString(string collectionPath, string propertyName, out string value) {
            throw new NotImplementedException();
        }

        public int GetStringOrDefault(string collectionPath, string propertyName, string defaultValue, out string value) {
            throw new NotImplementedException();
        }

        public int GetSubCollectionCount(string collectionPath, out uint subCollectionCount) {
            throw new NotImplementedException();
        }

        public int GetSubCollectionName(string collectionPath, uint index, out string subCollectionName) {
            throw new NotImplementedException();
        }

        public int GetUnsignedInt(string collectionPath, string propertyName, out uint value) {
            throw new NotImplementedException();
        }

        public int GetUnsignedInt64(string collectionPath, string propertyName, out ulong value) {
            throw new NotImplementedException();
        }

        public int GetUnsignedInt64OrDefault(string collectionPath, string propertyName, ulong defaultValue, out ulong value) {
            throw new NotImplementedException();
        }

        public int GetUnsignedIntOrDefault(string collectionPath, string propertyName, uint defaultValue, out uint value) {
            throw new NotImplementedException();
        }

        public int PropertyExists(string collectionPath, string propertyName, out int pfExists) {
            throw new NotImplementedException();
        }

        public int SetBinary(string collectionPath, string propertyName, uint byteLength, byte[] pBytes) {
            throw new NotImplementedException();
        }

        public int SetBool(string collectionPath, string propertyName, int value) {
            throw new NotImplementedException();
        }

        public int SetInt(string collectionPath, string propertyName, int value) {
            throw new NotImplementedException();
        }

        public int SetInt64(string collectionPath, string propertyName, long value) {
            throw new NotImplementedException();
        }

        public int SetString(string collectionPath, string propertyName, string value) {
            throw new NotImplementedException();
        }

        public int SetUnsignedInt(string collectionPath, string propertyName, uint value) {
            throw new NotImplementedException();
        }

        public int SetUnsignedInt64(string collectionPath, string propertyName, ulong value) {
            throw new NotImplementedException();
        }
    }
}
