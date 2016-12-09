// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Settings {
    public interface IRPersistentSettings: IRToolsSettings, IDisposable {
        void LoadSettings();
        Task SaveSettingsAsync();
    }
}
