using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.R.Core.Formatting;
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

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_ShowInternalFunctions")]
        [LocDescription("Settings_ShowInternalFunctions_Description")]
        [DefaultValue(true)]
        public bool ShowInternalFunctions
        {
            get { return REditorSettings.ShowInternalFunctions; }
            set { REditorSettings.ShowInternalFunctions = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_CommitOnSpace")]
        [LocDescription("Settings_CommitOnSpace_Description")]
        [DefaultValue(false)]
        public bool CommitOnSpace
        {
            get { return REditorSettings.CommitOnSpace; }
            set { REditorSettings.CommitOnSpace = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_ShowTclFunctions")]
        [LocDescription("Settings_ShowTclFunctions_Description")]
        [DefaultValue(false)]
        public bool ShowTclFunctions
        {
            get { return REditorSettings.ShowTclFunctions; }
            set { REditorSettings.ShowTclFunctions = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_BracesOnNewLine")]
        [LocDescription("Settings_BracesOnNewLine_Description")]
        [DefaultValue(false)]
        public bool BracesOnNewLine
        {
            get { return REditorSettings.FormatOptions.BracesOnNewLine; }
            set { REditorSettings.FormatOptions.BracesOnNewLine = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceAfterKeyword")]
        [LocDescription("Settings_SpaceAfterKeyword_Description")]
        [DefaultValue(true)]
        public bool SpaceAfterKeyword
        {
            get { return REditorSettings.FormatOptions.SpaceAfterKeyword; }
            set { REditorSettings.FormatOptions.SpaceAfterKeyword = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceAfterComma")]
        [LocDescription("Settings_SpaceAfterComma_Description")]
        [DefaultValue(true)]
        public bool SpaceAfterComma
        {
            get { return REditorSettings.FormatOptions.SpaceAfterComma; }
            set { REditorSettings.FormatOptions.SpaceAfterComma = value; }
        }

        public override void ResetSettings()
        {
            REditorSettings.ResetSettings();
            base.ResetSettings();
        }
    }
}
