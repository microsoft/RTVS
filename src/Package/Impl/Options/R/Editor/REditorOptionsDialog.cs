using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor
{
    [Guid("970B289E-7CCB-44FD-BA0E-514C165750DF")]
    [ComVisible(true)]
    public class REditorOptionsDialog : DialogPage
    {
        public REditorOptionsDialog()
        {
            this.SettingsRegistryPath = @"UserSettings\R_Advanced";
        }

        [LocCategory("Settings_PasteCategory")]
        [CustomLocDisplayName("Settings_FormatOnPaste")]
        [LocDescription("Settings_FormatOnPaste_Description")]
        [DefaultValue(true)]
        public bool FormatOnPaste
        {
            get { return REditorSettings.FormatOnPaste; }
            set { REditorSettings.FormatOnPaste = value; }
        }

        [LocCategory("Settings_ValidationCategory")]
        [CustomLocDisplayName("Settings_EnableValidation")]
        [LocDescription("Settings_EnableValidation_Description")]
        [DefaultValue(true)]
        public bool EnableValidation
        {
            get { return REditorSettings.ValidationEnabled; }
            set { REditorSettings.ValidationEnabled = value; }
        }

        public override void ResetSettings()
        {
            REditorSettings.ResetSettings();
            base.ResetSettings();
        }
    }
}
