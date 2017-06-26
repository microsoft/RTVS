// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor {
    public class REditorOptionsDialog : DialogPage {
        private readonly IWritableREditorSettings _settings;

        public REditorOptionsDialog() {
            SettingsRegistryPath = @"UserSettings\R_Tools";
            _settings = VsAppShell.Current.GetService<IWritableREditorSettings>();
        }

        [LocCategory("Settings_SyntaxCheckCategory")]
        [CustomLocDisplayName("Settings_EnableSyntaxCheck")]
        [LocDescription("Settings_EnableSyntaxCheck_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool EnableValidation {
            get => _settings.SyntaxCheckEnabled;
            set => _settings.SyntaxCheckEnabled = value;
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_CommitOnSpace")]
        [LocDescription("Settings_CommitOnSpace_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool CommitOnSpace {
            get => _settings.CommitOnSpace;
            set => _settings.CommitOnSpace = value;
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_CommitOnEnter")]
        [LocDescription("Settings_CommitOnEnter_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool CommitOnEnter {
            get => _settings.CommitOnEnter;
            set => _settings.CommitOnEnter = value;
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_TriggerOnFirstChar")]
        [LocDescription("Settings_TriggerOnFirstChar_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool TriggerOnFirstChar {
            get => _settings.ShowCompletionOnFirstChar;
            set => _settings.ShowCompletionOnFirstChar = value;
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_TriggerOnTab")]
        [LocDescription("Settings_TriggerOnTab_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool TriggerOnTab {
            get => _settings.ShowCompletionOnTab;
            set => _settings.ShowCompletionOnTab = value;
        }

        [LocCategory("Settings_IntellisenseCategory")]
        [CustomLocDisplayName("Settings_PartialArgumentNameMatch")]
        [LocDescription("Settings_PartialArgumentNameMatch_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool PartialArgumentNameMatch {
            get => _settings.PartialArgumentNameMatch;
            set => _settings.PartialArgumentNameMatch = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_BracesExpanded")]
        [LocDescription("Settings_BracesExpanded_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool BracesOnNewLine {
            get => _settings.FormatOptions.BracesOnNewLine;
            set => _settings.FormatOptions.BracesOnNewLine = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceAfterKeyword")]
        [LocDescription("Settings_SpaceAfterKeyword_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool SpaceAfterKeyword {
            get => _settings.FormatOptions.SpaceAfterKeyword;
            set => _settings.FormatOptions.SpaceAfterKeyword = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceAfterComma")]
        [LocDescription("Settings_SpaceAfterComma_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool SpaceAfterComma {
            get => _settings.FormatOptions.SpaceAfterComma;
            set => _settings.FormatOptions.SpaceAfterComma = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpaceBeforeCurly")]
        [LocDescription("Settings_SpaceBeforeCurly_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool SpaceBeforeCurly {
            get => _settings.FormatOptions.SpaceBeforeCurly;
            set => _settings.FormatOptions.SpaceBeforeCurly = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_SpacesAroundEquals")]
        [LocDescription("Settings_SpacesAroundEquals_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool SpacesAroundEquals {
            get => _settings.FormatOptions.SpacesAroundEquals;
            set => _settings.FormatOptions.SpacesAroundEquals = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_FormatOnPaste")]
        [LocDescription("Settings_FormatOnPaste_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool FormatOnPaste {
            get => _settings.FormatOnPaste;
            set => _settings.FormatOnPaste = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_AutoFormat")]
        [LocDescription("Settings_AutoFormat_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool AutoFormat {
            get => _settings.AutoFormat;
            set => _settings.AutoFormat = value;
        }

        [LocCategory("Settings_FormattingCategory")]
        [CustomLocDisplayName("Settings_FormatScope")]
        [LocDescription("Settings_FormatScope_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool FormatScope {
            get => _settings.FormatScope;
            set => _settings.FormatScope = value;
        }

        [LocCategory("Settings_ReplCategory")]
        [CustomLocDisplayName("Settings_ReplSyntaxCheck")]
        [LocDescription("Settings_ReplSyntaxCheck_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(false)]
        public bool SyntaxCheckInRepl {
            get => _settings.SyntaxCheckInRepl;
            set => _settings.SyntaxCheckInRepl = value;
        }

        [LocCategory("Settings_OutliningCategory")]
        [CustomLocDisplayName("Setting_EnableOutlining")]
        [LocDescription("Setting_EnableOutlining_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool EnableOutlining
        {
            get => _settings.EnableOutlining;
            set => _settings.EnableOutlining = value;
        }

        public override void ResetSettings() {
            _settings.ResetSettings();
            base.ResetSettings();
        }

        protected override void OnApply(PageApplyEventArgs e) {
            if (e.ApplyBehavior == ApplyKind.Apply) {
                RtvsTelemetry.Current.ReportSettings();
            }
            base.OnApply(e);
        }

        protected override void OnClosed(EventArgs e) => base.OnClosed(e);
    }
}
