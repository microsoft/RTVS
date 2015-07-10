using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Editor.Tree
{
    [ExcludeFromCodeCoverage]
    internal class EditorTreeChange
    {
        public TreeChangeType ChangeType { get; private set; }

        public EditorTreeChange(TreeChangeType changeType)
        {
            ChangeType = changeType;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class EditorTreeChange_NewTree : EditorTreeChange
    {
        public AstRoot NewTree { get; private set; }

        public EditorTreeChange_NewTree(AstRoot newTree)
            : base(TreeChangeType.NewTree)
        {
            NewTree = newTree;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class EditorTreeChange_TokenNodeChanged : EditorTreeChange
    {
        public int NodeKey { get; private set; }

        public EditorTreeChange_TokenNodeChanged(int affectedNodeKey)
            : base(TreeChangeType.TokenChange)
        {
            NodeKey = affectedNodeKey;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class EditorTreeChange_NodesChanged : EditorTreeChange
    {
        public int NodeKey { get; private set; }

        public IReadOnlyCollection<IAstNode> NewChildren { get; private set; }
        public IReadOnlyCollection<IAstNode> AddedElements { get; private set; }
        public IReadOnlyCollection<IAstNode> RemovedElements { get; private set; }

        public EditorTreeChange_NodesChanged(
            int affectedNodeKey,
            IReadOnlyCollection<IAstNode> newChildren,
            IReadOnlyCollection<IAstNode> addedElements,
            IReadOnlyCollection<IAstNode> removedElements)
            : base(TreeChangeType.NodesChanged)
        {
            NodeKey = affectedNodeKey;
            NewChildren = newChildren.Count > 0 ? newChildren : ReadOnlyTextRangeCollection<IAstNode>.EmptyCollection;
            AddedElements = addedElements.Count > 0 ? addedElements : ReadOnlyTextRangeCollection<IAstNode>.EmptyCollection;
            RemovedElements = removedElements.Count > 0 ? removedElements : ReadOnlyTextRangeCollection<IAstNode>.EmptyCollection;
        }
    }
    internal class EditorTreeChanges
    {
        public Queue<EditorTreeChange> ChangeQueue { get; private set; }
        public int SnapshotVersion { get; private set; }
        public bool FullParse { get; private set; }

        public EditorTreeChanges(int _snapshotVersion, bool fullParse)
            : this(new Queue<EditorTreeChange>(), _snapshotVersion, fullParse)
        {
        }

        public EditorTreeChanges(Queue<EditorTreeChange> changes, int _snapshotVersion, bool fullParse)
        {
            ChangeQueue = changes;
            SnapshotVersion = _snapshotVersion;
            FullParse = fullParse;
        }
    }
}
