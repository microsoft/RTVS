// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    public sealed class ViewCaret : IViewCaret {
        public ViewCaret(ITextCaret caret) {
            InVirtualSpace = caret.InVirtualSpace;
            var p = caret.Position.BufferPosition;
            Position = new CaretPosition(new EditorSnapshotPoint(p.Snapshot.TextBuffer.ToEditorBuffer().CurrentSnapshot, p), caret.Position.VirtualSpaces);
        }
        public bool InVirtualSpace { get; }
        public IViewCaretPosition Position { get; }
    }
}
