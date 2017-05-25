// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Settings;

namespace Microsoft.Language.Editor.Test.Settings {
    [ExcludeFromCodeCoverage]
    public class TestSettingsStorage : IWritableEditorSettingsStorage {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        #region IEditorSettingsStorage
        public T Get<T>(string name, T defaultValue) {
            object value;
            return _settings.TryGetValue(name, out value) ? (T)value : defaultValue;
        }
        #endregion 

        #region IWritableEditorSettingsStorage
        public void Set<T>(string name, T value) => _settings[name] = value;
        #endregion

        public void ResetSettings() { }
        public void Dispose() { }

#pragma warning disable 67
        public event EventHandler<EventArgs> SettingsChanged;
    }
}
