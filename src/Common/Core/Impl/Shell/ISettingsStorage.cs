// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Shell {
    public interface ISettingsStorage {
        bool SettingExists(string name);
        object GetSetting(string name, Type t);
        T GetSetting<T>(string name, T defaultValue);
        void SetSetting(string name, object value);
        Task PersistAsync();
    }
}
