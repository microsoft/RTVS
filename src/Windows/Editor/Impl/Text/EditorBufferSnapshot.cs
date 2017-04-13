// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public sealed class EditorBufferSnapshot: TextProvider, IEditorBufferSnapshot {
        private readonly ITextSnapshot _snapshot;
        public EditorBufferSnapshot(IEditorBuffer editorBuffer, ITextSnapshot snapshot): base(snapshot) {
            EditorBuffer = editorBuffer;
            _snapshot = snapshot;
        }

        public T As<T>() where T:class => _snapshot as T;
        public IEditorBuffer EditorBuffer { get; }
        public int LineCount => _snapshot.LineCount;
        public IEditorLine GetLineFromLineNumber(int lineNumber) => new EditorLine(EditorBuffer, _snapshot.GetLineFromLineNumber(lineNumber));
        public IEditorLine GetLineFromPosition(int position) => new EditorLine(EditorBuffer, _snapshot.GetLineFromPosition(position));
        public int GetLineNumberFromPosition(int position) => _snapshot.GetLineNumberFromPosition(position);
    }
}
