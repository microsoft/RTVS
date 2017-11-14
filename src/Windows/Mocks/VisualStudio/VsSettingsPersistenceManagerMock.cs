// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Settings;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsSettingsPersistenceManagerMock : ISettingsManager {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public ISettingsList GetOrCreateList(string name, bool isMachineLocal) => Substitute.For<ISettingsList>();
        public ISettingsSubset GetSubset(string namePattern) => Substitute.For<ISettingsSubset>();

        public T GetValueOrDefault<T>(string name, T defaultValue = default(T)) {
            object value;
            if(_settings.TryGetValue(name, out value)) {
                return (T)value;
            }
            return defaultValue;
        }

        public string[] NamesStartingWith(string prefix) {
            throw new NotImplementedException();
        }

        public void SetOnlineStore(IAsyncStringStorage store) {
            throw new NotImplementedException();
        }

        public void SetSharedStore(IAsyncStringStorage store) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task SetValueAsync(string name, object value, bool isMachineLocal) {
            _settings[name] = value;
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public GetValueResult TryGetValue<T>(string name, out T value) {
            object v;
            value = default(T);
            if (_settings.TryGetValue(name, out v)) {
                value = (T)v;
                return GetValueResult.Success;
            }
            return GetValueResult.Missing;
        }
    }
}
