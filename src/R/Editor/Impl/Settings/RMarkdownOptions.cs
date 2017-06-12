// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor {
    public enum RMarkdownPreviewPosition {
        Right,
        Below
    }

    public sealed class RMarkdownOptions {
        public const RMarkdownPreviewPosition DefaultPosition = RMarkdownPreviewPosition.Right;
        public const int DefaultWidth = 500;
        public const int DefaultHeight = 400;

        public bool EnablePreview { get; set; } = true;
        public RMarkdownPreviewPosition PreviewPosition { get; set; } = DefaultPosition;
        public bool AutomaticSync { get; set; } = true;
        public int PreviewWidth { get; set; } = DefaultWidth;
        public int PreviewHeight { get; set; } = DefaultHeight;
        public string CustomStylesheetFileName { get; set; }
    }
}
