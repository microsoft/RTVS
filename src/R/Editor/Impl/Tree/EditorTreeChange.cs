// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST;

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

    //[ExcludeFromCodeCoverage]
    //internal class EditorTreeChange_ScopeChanged : EditorTreeChange
    //{
    //    public IAstNode ScopeNode { get; private set; }

    //    public IReadOnlyCollection<IAstNode> NewChildren { get; private set; }

    //    public EditorTreeChange_ScopeChanged(IAstNode scopeNode, IReadOnlyCollection<IAstNode> newChildren)
    //        : base(TreeChangeType.ScopeChanged)
    //    {
    //        ScopeNode = scopeNode;
    //        NewChildren = newChildren.Count > 0 ? newChildren : ReadOnlyTextRangeCollection<IAstNode>.EmptyCollection;
    //    }
    //}
}
