// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.LanguageServer.Services.Editor;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorView : ServiceAndPropertyHolder, IEditorView {
        public EditorView(IEditorBuffer editorBuffer, int caretPosition) {
            EditorBuffer = editorBuffer;
            Caret = new ViewCaret(this, caretPosition);
        }
        public T As<T>() where T : class => throw new NotSupportedException();

        public IEditorBuffer EditorBuffer { get; }

        public IViewCaret Caret { get; }

        public ISnapshotPoint GetCaretPosition(IEditorBuffer buffer = null)
            => new SnapshotPoint(EditorBuffer.CurrentSnapshot, Caret.Position.Position);

        public IEditorSelection Selection => new EditorSelection(TextRange.EmptyRange);

        public ISnapshotPoint MapToView(IEditorBufferSnapshot snapshot, int position)
            => new SnapshotPoint(EditorBuffer.CurrentSnapshot, position);
    }
}
