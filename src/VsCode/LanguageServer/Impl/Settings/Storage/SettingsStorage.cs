// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class SettingsStorage : ISettingsStorage {
        public void Set<T>(string name, T value) { }
        public bool SettingExists(string name) => false;

        public object GetSetting(string name, Type t) => null;

        public T GetSetting<T>(string name, T defaultValue) => defaultValue;

        public void SetSetting(string name, object value) { }

        public Task PersistAsync() => Task.CompletedTask;
    }
}
