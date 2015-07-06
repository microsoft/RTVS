using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    [Guid("2E3086D0-9BEC-43EA-8F2F-1126A98AF013")]
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
            get { return RSettings.FormatOnPaste; }
            set { RSettings.FormatOnPaste = value; }
        }

        [LocCategory("Settings_ValidationCategory")]
        [CustomLocDisplayName("Settings_EnableValidation")]
        [LocDescription("Settings_EnableValidation_Description")]
        [DefaultValue(true)]
        public bool EnableValidation
        {
            get { return RSettings.ValidationEnabled; }
            set { RSettings.ValidationEnabled = value; }
        }

        public override void ResetSettings()
        {
            RSettings.ResetSettings();
            base.ResetSettings();
        }
    }
}
