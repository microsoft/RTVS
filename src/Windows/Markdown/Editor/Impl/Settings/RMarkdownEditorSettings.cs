// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Markdown.Editor.ContentTypes;

namespace Microsoft.Markdown.Editor.Settings {
    public sealed class RMarkdownEditorSettings : EditorSettings, IRMarkdownEditorSettings {
        public const string EnablePreviewKey = "EnablePreview";
        public const string PreviewPositionKey = "PreviewPosition";
        public const string AutomaticSyncKey = "AutomaticSync";
        public const string PreviewHeightKey = "PreviewHeight";
        public const string PreviewWidthKey = "PreviewWidth";
        public const string CustomStylesheetFileNameKey = "CustomStylesheetFileName";

        public const RMarkdownPreviewPosition DefaultPosition = RMarkdownPreviewPosition.Right;
        public const int DefaultWidth = 500;
        public const int DefaultHeight = 400;

        public RMarkdownEditorSettings(ICoreShell shell) : base(shell, MdContentTypeDefinition.LanguageName) { }
        public RMarkdownEditorSettings(IWritableEditorSettingsStorage storage) : base(storage) { }

        public bool EnablePreview {
            get => Storage.Get(EnablePreviewKey, true);
            set => WritableStorage?.Set(EnablePreviewKey, value);
        }

        public RMarkdownPreviewPosition PreviewPosition {
            get => (RMarkdownPreviewPosition)Storage.Get(PreviewPositionKey, (int)DefaultPosition);
            set => WritableStorage?.Set(PreviewPositionKey, (int)value);
        }

        public bool AutomaticSync {
            get => Storage.Get(AutomaticSyncKey, true);
            set => WritableStorage?.Set(AutomaticSyncKey, value);
        }

        public int PreviewWidth {
            get => Storage.Get(PreviewWidthKey, DefaultWidth);
            set => WritableStorage?.Set(PreviewWidthKey, value);
        }

        public int PreviewHeight {
            get => Storage.Get(PreviewHeightKey, DefaultHeight);
            set => WritableStorage?.Set(PreviewHeightKey, value);
        }

        public string CustomStylesheetFileName {
            get => Storage.Get(CustomStylesheetFileNameKey, (string)null);
            set => WritableStorage?.Set(CustomStylesheetFileNameKey, value);
        }

        public override void ResetSettings() {
            EnablePreview = true;
            PreviewPosition = DefaultPosition;
            AutomaticSync = true;
            PreviewHeight = DefaultHeight;
            PreviewWidth = DefaultWidth;
            CustomStylesheetFileName = null;
        }
    }
}
