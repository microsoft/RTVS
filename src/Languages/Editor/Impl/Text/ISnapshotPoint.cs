// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Represents position in a given text buffer snapshot
    /// </summary>
    public interface ISnapshotPoint {
        int Position { get; }
        IEditorBufferSnapshot Snapshot { get; }
        IEditorLine GetContainingLine();
    }
}
