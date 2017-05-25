// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Settings {
    /// <summary>
    /// Provides language specific (editor) settings storage. 
    /// Exported via MEF for a particular content type.
    /// </summary>
    public interface IEditorSettingsStorageProvider {
        IEditorSettingsStorage GetSettingsStorage();
    }
}
