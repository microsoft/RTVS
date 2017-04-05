// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Settings;

namespace Microsoft.Language.Editor.Test.Settings {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableEditorSettingsStorage))]
    public class TestSettingsStorage : IWritableEditorSettingsStorage {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public void BeginBatchChange() { }

        public void EndBatchChange() { }

        public bool GetBoolean(string name, bool defaultValue)
            => _settings.ContainsKey(name) ? (bool)_settings[name] : defaultValue;

        public int GetInteger(string name, int defaultValue)
            => _settings.ContainsKey(name) ? (int)_settings[name] : defaultValue;

        public string GetString(string name, string defaultValue)
            => _settings.ContainsKey(name) ? (string)_settings[name] : defaultValue;

        public void LoadFromStorage() { }
        public void ResetSettings() { }

        public void SetBoolean(string name, bool value) => _settings[name] = value;
        public void SetInteger(string name, int value) => _settings[name] = value;
        public void SetString(string name, string value) => _settings[name] = value;

#pragma warning disable 67
        public event EventHandler<EventArgs> SettingsChanged;
    }
}
