// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Describes complete context of the text change including text ranges, 
    /// changes accumulated so far and the affected editor tree.
    /// and changed AST node.
    /// </summary>
    internal sealed class TextChangeContext {
        /// <summary>
        /// Editor tree associated with the changing buffer
        /// </summary>
        public IREditorTree EditorTree { get; }

        /// <summary>
        /// Changes accumulated since last tree update
        /// </summary>
        public TreeTextChange PendingChanges { get; }

        /// <summary>
        /// Most recently changed node (if change was AST node change)
        /// </summary>
        public IAstNode ChangedNode { get; set; }

        /// <summary>
        /// Most recently changed comment (if change was inside comments)
        /// </summary>
        public RToken ChangedComment { get; set; }

        public TextChangeContext(IREditorTree editorTree, TreeTextChange change, TreeTextChange pendingChanges) {
            EditorTree = editorTree;

            TreeTextChange ttc;
            if (change.OldTextProvider == null || change.NewTextProvider == null) {
                var oldTextProvider = change.OldTextProvider ?? editorTree.AstRoot.TextProvider;
                var newTextProvider = change.NewTextProvider ?? editorTree.AstRoot.TextProvider;
                ttc = new TreeTextChange(change.Start, change.OldLength, change.NewLength, oldTextProvider, newTextProvider);
            } else {
                ttc = change;
            }

            PendingChanges = pendingChanges;
            PendingChanges.Combine(ttc);
        }
    }
}
