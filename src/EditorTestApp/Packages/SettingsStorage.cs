using System;
using Microsoft.Languages.Editor.Settings;

namespace Microsoft.Languages.Editor.Application.Packages
{
    internal class SettingsStorage : IWritableEditorSettingsStorage
    {
        #region IWebEditorSettingsStorage Members

        public virtual string GetString(string name, string defaultValue = "")
        {
            return defaultValue;
        }

        public virtual int GetInteger(string name, int defaultValue = 0)
        {
            return defaultValue;
        }

        public virtual bool GetBoolean(string name, bool defaultValue = true)
        {
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
        }

        public void SetInteger(string name, int value)
        {
        }

        public void SetBoolean(string name, bool value)
        {
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
