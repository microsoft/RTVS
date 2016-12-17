// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor {
    public class REditorOptionsDialog : DialogPage {
        public REditorOptionsDialog() {
            this.SettingsRegistryPath = @"UserSettings\R_Tools";
        }

        [LocCategory("Settings_SyntaxCheckCategory")]
        [CustomLocDisplayName("Settings_EnableSyntaxCheck")]
        [LocDescription("Settings_EnableSyntaxCheck_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool EnableValidation {
            get { return REditorSettings.SyntaxCheck; }
            set { REditorSettings.SyntaxCheck = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_CommitOnSpace")]
        [LocDescription("Settings_CommitOnSpace_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool CommitOnSpace {
            get { return REditorSettings.CommitOnSpace; }
            set { REditorSettings.CommitOnSpace = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_CommitOnEnter")]
        [LocDescription("Settings_CommitOnEnter_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool CommitOnEnter {
            get { return REditorSettings.CommitOnEnter; }
            set { REditorSettings.CommitOnEnter = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_TriggerOnFirstChar")]
        [LocDescription("Settings_TriggerOnFirstChar_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool TriggerOnFirstChar {
            get { return REditorSettings.ShowCompletionOnFirstChar; }
            set { REditorSettings.ShowCompletionOnFirstChar = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_TriggerOnTab")]
        [LocDescription("Settings_TriggerOnTab_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool TriggerOnTab {
            get { return REditorSettings.ShowCompletionOnTab; }
            set { REditorSettings.ShowCompletionOnTab = value; }
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_PartialArgumentNameMatch")]
        [LocDescription("Settings_PartialArgumentNameMatch_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool PartialArgumentNameMatch {
            get { return REditorSettings.PartialArgumentNameMatch; }
            set { REditorSettings.PartialArgumentNameMatch = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_BracesExpanded")]
        [LocDescription("Settings_BracesExpanded_Description")]
        [DefaultValue(false)]
        public bool BracesOnNewLine {
            get { return REditorSettings.FormatOptions.BracesOnNewLine; }
            set { REditorSettings.FormatOptions.BracesOnNewLine = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceAfterKeyword")]
        [LocDescription("Settings_SpaceAfterKeyword_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool SpaceAfterKeyword {
            get { return REditorSettings.FormatOptions.SpaceAfterKeyword; }
            set { REditorSettings.FormatOptions.SpaceAfterKeyword = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceAfterComma")]
        [LocDescription("Settings_SpaceAfterComma_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool SpaceAfterComma {
            get { return REditorSettings.FormatOptions.SpaceAfterComma; }
            set { REditorSettings.FormatOptions.SpaceAfterComma = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_FormatOnPaste")]
        [LocDescription("Settings_FormatOnPaste_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool FormatOnPaste {
            get { return REditorSettings.FormatOnPaste; }
            set { REditorSettings.FormatOnPaste = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_AutoFormat")]
        [LocDescription("Settings_AutoFormat_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool AutoFormat {
            get { return REditorSettings.AutoFormat; }
            set { REditorSettings.AutoFormat = value; }
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_FormatScope")]
        [LocDescription("Settings_FormatScope_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool FormatScope {
            get { return REditorSettings.FormatScope; }
            set { REditorSettings.FormatScope = value; }
        }

        [LocCategory("Settings_ReplCategory")]
        [CustomLocDisplayName("Settings_ReplSyntaxCheck")]
        [LocDescription("Settings_ReplSyntaxCheck_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool SyntaxCheckInRepl {
            get { return REditorSettings.SyntaxCheckInRepl; }
            set { REditorSettings.SyntaxCheckInRepl = value; }
        }

        [LocCategory("Settings_OutliningCategory")]
        [CustomLocDisplayName("Setting_EnableOutlining")]
        [LocDescription("Setting_EnableOutlining_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool EnableOutlining
        {
            get { return REditorSettings.EnableOutlining; }
            set { REditorSettings.EnableOutlining = value; }
        }

        public override void ResetSettings() {
            REditorSettings.ResetSettings();
            base.ResetSettings();
        }

        protected override void OnApply(PageApplyEventArgs e) {
            if (e.ApplyBehavior == ApplyKind.Apply) {
                RtvsTelemetry.Current.ReportSettings();
            }
            base.OnApply(e);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
        }
    }
}
