// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Settings {
    /// <summary>
    /// Writable settings storage. Exported via MEF for a particular content type.
    /// Editor uses exported object to store settings such as indentation style, 
    /// tab size, formatting options and so on.
    /// </summary>
    public interface IWritableEditorSettingsStorage : IEditorSettingsStorage {
        void ResetSettings();
        void Set<T>(string name, T value);
    }
}
