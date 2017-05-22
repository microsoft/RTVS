// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Settings {
    /// <summary>
    /// Editor settings storage.
    /// </summary>
    public interface IEditorSettingsStorage: IDisposable {
        event EventHandler<EventArgs> SettingsChanged;
        T Get<T>(string name, T defaultValue);
    }
}
