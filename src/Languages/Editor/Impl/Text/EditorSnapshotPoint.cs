// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Text {
    public sealed class EditorSnapshotPoint : ISnapshotPoint {
        public EditorSnapshotPoint(IEditorBufferSnapshot snapshot, int position) {
            Snapshot = snapshot;
            Position = position;
        }

        public int Position { get; }
        public IEditorBufferSnapshot Snapshot { get; }
        public IEditorLine GetContainingLine() => Snapshot.GetLineFromPosition(Position);
    }
}
