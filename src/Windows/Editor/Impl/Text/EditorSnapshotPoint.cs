// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public sealed class EditorSnapshotPoint : ISnapshotPoint {
        private readonly SnapshotPoint _point;
        private readonly IEditorBuffer _editorBuffer;

        public EditorSnapshotPoint(SnapshotPoint point, IEditorBuffer editorBuffer) {
            Check.Argument(nameof(point), () => point.Snapshot.TextBuffer == editorBuffer.As<ITextBuffer>());
            _point = point;
            _editorBuffer = editorBuffer;
        }

        public int Position => _point.Position;
        public IBufferSnapshot Snapshot => new EditorBufferSnapshot(_editorBuffer, _point.Snapshot);
        public IEditorLine GetContainingLine() => new EditorLine(_editorBuffer, _point.GetContainingLine());
    }
}
