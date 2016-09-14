// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Settings;

namespace Microsoft.Languages.Editor.Application.Packages {
    [ExcludeFromCodeCoverage]
    internal class SettingsStorage : IWritableSettingsStorage
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        #region IWebEditorSettingsStorage Members

        public virtual string GetString(string name, string defaultValue = "")
        {
            if (_settings.ContainsKey(name))
                return _settings[name] as string;

            return defaultValue;
        }

        public virtual int GetInteger(string name, int defaultValue = 0)
        {
            if (_settings.ContainsKey(name))
                return (int)_settings[name];

            return defaultValue;
        }

        public virtual bool GetBoolean(string name, bool defaultValue = true)
        {
            if (_settings.ContainsKey(name))
                return (bool)_settings[name];

            return defaultValue;
        }

        public virtual byte[] GetBytes(string name)
        {
            return new byte[0];
        }

        #endregion

        #region IWebEditorSettingsStorageEvents Members

        #pragma warning disable 0067
        public event EventHandler<EventArgs> SettingsChanged;

        #endregion

        #region IWritableWebEditorSettingsStorage Members

        public void SetString(string name, string value)
        {
            _settings[name] = value;
        }

        public void SetInteger(string name, int value)
        {
            _settings[name] = value;
        }

        public void SetBoolean(string name, bool value)
        {
            _settings[name] = value;
        }

        public void SetBytes(string name, byte[] value)
        {
        }

        public void BeginBatchChange()
        {
        }

        public void EndBatchChange()
        {
        }

        public void ResetSettings()
        {
        }

        public void LoadFromStorage()
        {
        }
        #endregion
    }
}
