// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.Markdown {
    public class RMarkdownOptionsDialog : DialogPage {
        private readonly IRMarkdownEditorSettings _settings;

        public RMarkdownOptionsDialog() {
            SettingsRegistryPath = @"UserSettings\R_Markdown";
            _settings = VsAppShell.Current.GetService<IRMarkdownEditorSettings>();
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_EnablePreview")]
        [LocDescription("Settings_Markdown_EnablePreview_Description")]
        [DefaultValue(true)]
        public bool EnablePreview {
            get => _settings.EnablePreview;
            set => _settings.EnablePreview = value;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_PreviewPosition")]
        [LocDescription("Settings_Markdown_PreviewPosition_Description")]
        [TypeConverter(typeof(PreviewPositionTypeConverter))]
        [DefaultValue(RMarkdownEditorSettings.DefaultPosition)]
        public RMarkdownPreviewPosition PreviewPosition {
            get => _settings.PreviewPosition;
            set => _settings.PreviewPosition = value;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_AutomaticSync")]
        [LocDescription("Settings_Markdown_AutomaticSync_Description")]
        [DefaultValue(true)]
        public bool AutomaticSync {
            get => _settings.AutomaticSync;
            set => _settings.AutomaticSync = value;
        }

        [LocCategory("Settings_MarkdownCategory_Scroll")]
        [CustomLocDisplayName("Settings_Markdown_ScrollEditorWithPreview")]
        [LocDescription("Settings_Markdown_ScrollEditorWithPreview_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool ScrollEditorWithPreview {
            get => _settings.ScrollEditorWithPreview;
            set => _settings.ScrollEditorWithPreview = value;
        }

        [LocCategory("Settings_MarkdownCategory_Scroll")]
        [CustomLocDisplayName("Settings_Markdown_ScrollPreviewWithEditor")]
        [LocDescription("Settings_Markdown_ScrollPreviewWithEditor_Description")]
        [TypeConverter(typeof(OnOffTypeConverter))]
        [DefaultValue(true)]
        public bool ScrollPreviewWithEditor {
            get => _settings.ScrollPreviewWithEditor;
            set => _settings.ScrollPreviewWithEditor = value;
        }

        [Browsable(false)]
        public int PreviewWidth {
            get => _settings.PreviewWidth;
            set => _settings.PreviewWidth = value;
        }

        [Browsable(false)]
        public int PreviewHeight {
            get => _settings.PreviewHeight;
            set => _settings.PreviewHeight = value;
        }

        public override void ResetSettings() {
            _settings.ResetSettings();
            base.ResetSettings();
        }
    }
}
