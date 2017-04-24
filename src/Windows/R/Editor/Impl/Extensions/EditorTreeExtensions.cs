// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Tree {
    public static class EditorTreeExtensions {
        public static ITextSnapshot TextSnapshot(this IREditorTree tree) => tree.BufferSnapshot.As<ITextSnapshot>();
        public static ITextBuffer TextBuffer(this IREditorTree tree) => tree.EditorBuffer.As<ITextBuffer>();
    }
}
