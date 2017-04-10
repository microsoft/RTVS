// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Tree {
    [ExcludeFromCodeCoverage]
    public class TreeUpdatePendingEventArgs : EventArgs {
        public TextChangeEventArgs Change { get; private set; }

        public TreeUpdatePendingEventArgs(TextChangeEventArgs change) {
            Change = change;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeUpdatedEventArgs : EventArgs {
        public TreeUpdateType UpdateType { get; private set; }

        public TreeUpdatedEventArgs(TreeUpdateType updateType) {
            UpdateType = updateType;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeNodesRemovedEventArgs : EventArgs {
        public IReadOnlyCollection<IAstNode> Nodes { get; private set; }

        public TreeNodesRemovedEventArgs(IReadOnlyCollection<IAstNode> nodes) {
            Nodes = nodes;
        }
    }

    //[ExcludeFromCodeCoverage]
    //public class TreeScopeChangedEventArgs : EventArgs
    //{
    //    public IAstNode ScopeNode { get; private set; }

    //    public TreeScopeChangedEventArgs(IAstNode scopeNode)
    //    {
    //        ScopeNode = scopeNode;
    //    }
    //}

    [ExcludeFromCodeCoverage]
    public class TreePositionsOnlyChangedEventArgs : EventArgs {
        public TreePositionsOnlyChangedEventArgs() {
        }
    }
}
