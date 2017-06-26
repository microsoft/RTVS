// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Markdown.Editor.Settings;

namespace Microsoft.Markdown.Editor.Test.Fixtures {
    internal sealed class TestMarkdownEditorSettings : IRMarkdownEditorSettings {
        public bool EnablePreview { get; set; }
        public RMarkdownPreviewPosition PreviewPosition { get; set; } = RMarkdownPreviewPosition.Right;
        public bool AutomaticSync { get; set; } = false;
        public int PreviewWidth { get; set; } = 400;
        public int PreviewHeight { get; set; } = 300;
        public void ResetSettings() { }
    }
}
