// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Languages.Editor.Settings;

namespace Microsoft.R.LanguageServer.Settings
{
    internal sealed class EditorSettingsStorage: IEditorSettingsStorage {
        public void Dispose() {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> SettingsChanged;

        public T Get<T>(string name, T defaultValue) {
            throw new NotImplementedException();
        }
    }
}
