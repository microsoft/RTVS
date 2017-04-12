// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Describes set of changes in the tree that were generated
    /// by the background parser. Changes are applied to the tree
    /// in the main thread in order to avoid unnecessary locks.
    /// </summary>
    internal sealed class EditorTreeChangeCollection {
        /// <summary>
        /// Changes to apply to the tree
        /// </summary>
        public Queue<EditorTreeChange> ChangeQueue { get; }

        /// <summary>
        /// Version of the text snaphot the changes were generated agaist.
        /// </summary>
        public int SnapshotVersion { get; }

        /// <summary>
        /// Indicates if full parse required
        /// </summary>
        public bool FullParseRequired { get; }

        public EditorTreeChangeCollection(int _snapshotVersion, bool fullParseRequired)
            : this(new Queue<EditorTreeChange>(), _snapshotVersion, fullParseRequired) {
        }

        public EditorTreeChangeCollection(Queue<EditorTreeChange> changes, int _snapshotVersion, bool fullParseRequired) {
            ChangeQueue = changes;
            SnapshotVersion = _snapshotVersion;
            FullParseRequired = fullParseRequired;
        }
    }
}
