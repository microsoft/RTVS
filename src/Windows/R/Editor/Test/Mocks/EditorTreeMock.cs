// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Tree;

namespace Microsoft.R.Editor.Test.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class EditorTreeMock : IREditorTree {
        public EditorTreeMock(IEditorBuffer editorBuffer, AstRoot ast) {
            EditorBuffer = editorBuffer;
            AstRoot = ast;
        }
        public AstRoot AstRoot { get; }
        public AstRoot PreviousAstRoot { get; private set; }

        public bool IsReady => true;

        public IEditorBuffer EditorBuffer { get; }
        public IEditorBufferSnapshot BufferSnapshot => EditorBuffer.CurrentSnapshot;

        public AstRoot AcquireReadLock(Guid treeUserId) => AstRoot;
        public void Invalidate() { }
        public void EnsureTreeReady() { }
        public bool ReleaseReadLock(Guid treeUserId) => true;
        public void ProcessChangesAsync(Action completeCallback) { }

        public void InvokeWhenReady(Action<object> action, object p, Type type, bool processNow = false) { }
        public IExpressionTermFilter ExpressionTermFilter => null;

        public void Dispose() { }
#pragma warning disable 67
        public event EventHandler<EventArgs> Closing;
        public event EventHandler<TreeNodesRemovedEventArgs> NodesRemoved;
        public event EventHandler<TreePositionsOnlyChangedEventArgs> PositionsOnlyChanged;
        public event EventHandler<EventArgs> UpdateBegin;
        public event EventHandler<TreeUpdatedEventArgs> UpdateCompleted;
        public event EventHandler<EventArgs> UpdatesPending;
    }
}
