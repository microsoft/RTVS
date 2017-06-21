// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Markdown.Editor.Settings {
    public interface IRMarkdownEditorSettings {
        bool EnablePreview { get; set; }
        RMarkdownPreviewPosition PreviewPosition { get; set; }
        bool AutomaticSync { get; set; }
        int PreviewWidth { get; set; }
        int PreviewHeight { get; set; }
        void ResetSettings();
    }
}
