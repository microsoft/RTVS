// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Tree {
    class TreeChangeEventRecord {
        public TreeChangeType ChangeType { get; private set; }

        public IAstNode Node { get; private set; }

        public TreeChangeEventRecord(TreeChangeType changeType) {
            ChangeType = changeType;
        }

        public TreeChangeEventRecord(TreeChangeType changeType, IAstNode node) :
            this(changeType) {
            Node = node;
        }
    }
}
