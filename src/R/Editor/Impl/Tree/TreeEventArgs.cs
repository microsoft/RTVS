// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Tree {
    public class TreeUpdatedEventArgs : EventArgs {
        public TreeUpdateType UpdateType { get; }
        public TreeUpdatedEventArgs(TreeUpdateType updateType) => UpdateType = updateType;
    }

    public class TreeNodesRemovedEventArgs : EventArgs {
        public IReadOnlyCollection<IAstNode> Nodes { get; }
        public TreeNodesRemovedEventArgs(IReadOnlyCollection<IAstNode> nodes) => Nodes = nodes;
    }

    public class TreePositionsOnlyChangedEventArgs : EventArgs { }
}
