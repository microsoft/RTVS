// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Components.Settings {
    public interface IRPersistentSettings: IRSettings, IDisposable {
        void LoadSettings();
        Task SaveSettingsAsync();
    }
}
