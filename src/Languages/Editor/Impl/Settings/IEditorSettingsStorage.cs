// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Settings {
    /// <summary>
    /// Settings storage. Exported via MEF for a particular content type.
    /// Editor uses exported object to retrieve its settings such as indentation
    /// style, tab size, formatting options and so on.
    /// </summary>
    public interface IEditorSettingsStorage {
        event EventHandler<EventArgs> SettingsChanged;
        void LoadFromStorage();
        T Get<T>(string name, T defaultValue);
    }
}
