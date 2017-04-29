// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public sealed class EditorSnapshotPoint : ISnapshotPoint {
        public EditorSnapshotPoint(ITextSnapshot snapshot, int position) {
            Snapshot = new EditorBufferSnapshot(snapshot);
            Position = position;
        }

        public int Position { get; }
        public IEditorBufferSnapshot Snapshot { get; }
        public IEditorLine GetContainingLine() => Snapshot.GetLineFromPosition(Position);
    }
}
