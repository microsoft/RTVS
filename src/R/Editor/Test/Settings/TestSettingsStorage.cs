using System;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Test.Settings
{
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Test Editor settings")]
    [Order(Before = "Default")]
    public class TestSettingsStorage : IWritableEditorSettingsStorage
    {
        public event EventHandler<EventArgs> SettingsChanged;

        public void BeginBatchChange()
        {
        }

        public void EndBatchChange()
        {
        }

        public bool GetBoolean(string name, bool defaultValue)
        {
            return defaultValue;
        }

        public int GetInteger(string name, int defaultValue)
        {
            return defaultValue;
        }

        public string GetString(string name, string defaultValue)
        {
            return defaultValue;
        }

        public void LoadFromStorage()
        {
        }

        public void ResetSettings()
        {
        }

        public void SetBoolean(string name, bool value)
        {
        }

        public void SetInteger(string name, int value)
        {
        }

        public void SetString(string name, string value)
        {
        }
    }
}
