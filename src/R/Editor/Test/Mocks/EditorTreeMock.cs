// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class EditorTreeMock : IEditorTree
    {
        public EditorTreeMock(ITextBuffer textBuffer, AstRoot ast)
        {
            TextBuffer = textBuffer;
            AstRoot = ast;
        }
        public AstRoot AstRoot { get; private set; }

        public bool IsReady
        {
            get { return true; }
        }

        public ITextBuffer TextBuffer { get; private set; }

        public ITextSnapshot TextSnapshot
        {
            get { return TextBuffer.CurrentSnapshot; }
        }

        public AstRoot AcquireReadLock(Guid treeUserId)
        {
            return AstRoot;
        }

        public int Invalidate()
        {
            return 1;
        }

        public void EnsureTreeReady()
        {
        }

        public bool ReleaseReadLock(Guid treeUserId)
        {
            return true;
        }

        public void ProcessChangesAsync(Action completeCallback) {
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> Closing;
        public event EventHandler<TreeNodesRemovedEventArgs> NodesRemoved;
        public event EventHandler<TreePositionsOnlyChangedEventArgs> PositionsOnlyChanged;
        public event EventHandler<EventArgs> UpdateBegin;
        public event EventHandler<TreeUpdatedEventArgs> UpdateCompleted;
        public event EventHandler<TreeUpdatePendingEventArgs> UpdatesPending;
    }
}
