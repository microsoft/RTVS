// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Text {
    public sealed class CaretPosition : IViewCaretPosition {
        private readonly ISnapshotPoint _point;

        public CaretPosition(ISnapshotPoint point, int virtualSpaces) {
            _point = point;
            VirtualSpaces = virtualSpaces;
        }
        public int Position => _point.Position;
        public IEditorBufferSnapshot Snapshot => _point.Snapshot;
        public int VirtualSpaces { get; }
        public IEditorLine GetContainingLine() => _point.GetContainingLine();
    }
}
