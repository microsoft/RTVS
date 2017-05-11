// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Tree {
    internal class EditorTreeChange {
        public TreeChangeType ChangeType { get; }
        public EditorTreeChange(TreeChangeType changeType) {
            ChangeType = changeType;
        }
    }

    internal class EditorTreeChange_NewTree : EditorTreeChange {
        public AstRoot NewTree { get; }
        public EditorTreeChange_NewTree(AstRoot newTree)
            : base(TreeChangeType.NewTree) {
            NewTree = newTree;
        }
    }
}
