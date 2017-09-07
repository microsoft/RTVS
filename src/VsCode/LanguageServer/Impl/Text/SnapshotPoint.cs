// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal class SnapshotPoint: ISnapshotPoint {
        public SnapshotPoint(IEditorBufferSnapshot snapshot, int position) {
            Snapshot = snapshot;
            Position = position;
        }

        public int Position { get; }
        public IEditorBufferSnapshot Snapshot { get; }
        public IEditorLine GetContainingLine() => Snapshot.GetLineFromPosition(Position);
    }
}
