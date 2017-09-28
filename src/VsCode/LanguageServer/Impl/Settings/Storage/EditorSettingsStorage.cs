// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Settings;

namespace Microsoft.R.LanguageServer.Settings {
    internal sealed class EditorSettingsStorage: IEditorSettingsStorage {
        public void Dispose() {
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> SettingsChanged;

        public T Get<T>(string name, T defaultValue) => defaultValue;
    }
}
