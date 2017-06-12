// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor {
    public class RMarkdownOptionsDialog : DialogPage {
        private readonly RMarkdownOptions _options;

        public RMarkdownOptionsDialog() {
            SettingsRegistryPath = @"UserSettings\R_Markdown";
            _options = VsAppShell.Current.GetService<IWritableREditorSettings>().MarkdownOptions;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_EnablePreview")]
        [LocDescription("Settings_Markdown_EnablePreview_Description")]
        [DefaultValue(true)]
        public bool EnablePreview {
            get => _options.EnablePreview;
            set => _options.EnablePreview = value;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_PreviewPosition")]
        [LocDescription("Settings_Markdown_PreviewPosition_Description")]
        [TypeConverter(typeof(PreviewPositionTypeConverter))]
        [DefaultValue(RMarkdownOptions.DefaultPosition)]
        public RMarkdownPreviewPosition PreviewPosition {
            get => _options.PreviewPosition;
            set => _options.PreviewPosition = value;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_AutomaticSync")]
        [LocDescription("Settings_Markdown_AutomaticSync_Description")]
        [DefaultValue(true)]
        public bool AutomaticSync {
            get => _options.AutomaticSync;
            set => _options.AutomaticSync = value;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_PreviewWidth")]
        [LocDescription("Settings_Markdown_PreviewWidth_Description")]
        [DefaultValue(RMarkdownOptions.DefaultWidth)]
        public int PreviewWidth {
            get => _options.PreviewWidth;
            set => _options.PreviewWidth = value;
        }

        [LocCategory("Settings_MarkdownCategory_Preview")]
        [CustomLocDisplayName("Settings_Markdown_PreviewHeight")]
        [LocDescription("Settings_Markdown_PreviewHeight_Description")]
        [DefaultValue(RMarkdownOptions.DefaultHeight)]
        public int PreviewHeight {
            get => _options.PreviewHeight;
            set => _options.PreviewHeight = value;
        }

        [LocCategory("Settings_MarkdownCategory_CSS")]
        [CustomLocDisplayName("Settings_Markdown_CustomStylesheet")]
        [LocDescription("Settings_Markdown_CustomStylesheet_Description")]
        public string CustomStylesheet {
            get => _options.CustomStylesheetFileName;
            set => _options.CustomStylesheetFileName = value;
        }
    }
}
