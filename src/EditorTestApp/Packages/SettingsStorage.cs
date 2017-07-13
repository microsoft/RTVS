// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Settings;

namespace Microsoft.Languages.Editor.Application.Packages {
    [ExcludeFromCodeCoverage]
    internal abstract class SettingsStorage : IWritableEditorSettingsStorage {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        #region IEditorSettingsStorage Members
        public T Get<T>(string name, T defaultValue = default(T)) => _settings.TryGetValue(name, out object value) ? (T)value : defaultValue;

#pragma warning disable 67
        public event EventHandler<EventArgs> SettingsChanged;
#pragma warning restore 67
        #endregion

        #region IWritableEditorSettingsStorage Members
        public void Set<T>(string name, T value) => _settings[name] = value;
        public void ResetSettings() { }
        public void Dispose() { }
        #endregion
    }
}
