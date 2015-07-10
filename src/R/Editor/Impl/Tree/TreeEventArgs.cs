using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Editor.Tree
{
    [ExcludeFromCodeCoverage]
    public class TreeUpdatePendingEventArgs : EventArgs
    {
        public IReadOnlyCollection<TextChangeEventArgs> TextChanges { get; private set; }

        public TreeUpdatePendingEventArgs(IReadOnlyCollection<TextChangeEventArgs> textChanges)
        {
            TextChanges = textChanges;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeUpdatedEventArgs : EventArgs
    {
        public TreeUpdateType UpdateType { get; private set; }
        public bool FullParse { get; private set; }

        public TreeUpdatedEventArgs(TreeUpdateType updateType, bool fullParse)
        {
            UpdateType = updateType;
            FullParse = fullParse;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeNodesRemovedEventArgs : EventArgs
    {
        public IReadOnlyCollection<IAstNode> Nodes { get; private set; }

        public TreeNodesRemovedEventArgs(IReadOnlyCollection<IAstNode> nodes)
        {
            Nodes = nodes;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeTokenNodeChangedEventArgs : EventArgs
    {
        public int NodeKey { get; private set; }

        public TreeTokenNodeChangedEventArgs(int nodeKey)
        {
            NodeKey = nodeKey;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeNodeChangedEventArgs : EventArgs
    {
        public IAstNode OldNode { get; private set; }

        public IAstNode NewNode { get; private set; }

        public TreeNodeChangedEventArgs(IAstNode oldNode, IAstNode newNode)
        {
            OldNode = oldNode;
            NewNode = newNode;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreePositionsOnlyChangedEventArgs : EventArgs
    {
        public int ElementKey { get; private set; }

        public TreePositionsOnlyChangedEventArgs(int elementKey)
        {
            ElementKey = elementKey;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreePositionEventArgs : EventArgs
    {
        public int StartingPosition { get; private set; }
        public int Offset { get; private set; }

        public TreePositionEventArgs(int startingPosition, int offset)
        {
            StartingPosition = startingPosition;
            Offset = offset;
        }
    }
}
