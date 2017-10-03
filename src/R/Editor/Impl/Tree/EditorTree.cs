// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using static System.FormattableString;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Document tree for the editor. It does not derive from AST and rather
    /// aggregates it. The reason is that editor tree can be read and updated from
    /// different threads and hence we need to control access to the tree elements
    /// using appropriate locks. Typically there are three threads: main application
    /// thread which should be creating editor tree, incremental parse thread and
    /// validation (syntx check) thread.
    /// </summary>
    public partial class EditorTree : IREditorTree {
        private const string NotMainThreadMessage = "Method should only be called on the main thread";
        private const string AcquireReadLockMessage = NotMainThreadMessage + " Use AcquireReadLock when accessing tree from a background thread.";

        #region IREditorTree
        /// <summary>
        /// Visual Studio core editor text buffer
        /// </summary>
        public IEditorBuffer EditorBuffer { get; private set; }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AcquireReadLock")]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public AstRoot AstRoot {
            get {
                Check.InvalidOperation(() => TreeLock.CheckAccess(), AcquireReadLockMessage);
                // Do not call EnsureTreeReady here since it may slow down
                // typing A LOT in large files. If some code needs up-to-date
                // tree it has to call EnsureTreeReady explicitly.
                return _astRoot;
            }
        }

        /// <summary>
        /// Returns true if tree matches current document snapshot
        /// </summary>
        public bool IsReady {
            get {
                if (IsDirty) {
                    return false;
                }
                if (EditorBuffer != null && BufferSnapshot != null) {
                    return EditorBuffer.CurrentSnapshot.Version == BufferSnapshot.Version;
                }
                return true;
            }
        }

        /// <summary>
        /// Last text snapshot associated with this tree
        /// </summary>
        public IEditorBufferSnapshot BufferSnapshot {
            get => _textSnapShot;
            internal set {
                _textSnapShot = value;
                if (_astRoot != null) {
                    _astRoot.TextProvider = _textSnapShot;
                }
            }
        }

        /// <summary>
        /// Ensures tree is up to date, matches current text buffer snapshot
        /// and all changes since the last update were processed. Blocks until 
        /// all changes have been processed. Does not pump messages.
        /// </summary>
        public void EnsureTreeReady() {
            if (TreeUpdateTask == null) {
                return;
            }
            Check.InvalidOperation(() => _ownerThread == Thread.CurrentThread.ManagedThreadId, NotMainThreadMessage);
            // OK to run in sync if changes are pending since we need it updated now
            TreeUpdateTask.EnsureProcessingComplete();
        }

        /// <summary>
        /// Provides a way to automatically invoke particular action
        /// once when tree becomes ready again. Typically used in
        /// asynchronous completion and signature help scenarios.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="p">Parameter to pass to the action</param>
        /// <param name="type">Action identifier</param>
        /// <param name="processNow">
        /// If true, change processing begins now. 
        /// If false, next regular parsing pass with process pending changes.
        /// </param>
        public void InvokeWhenReady(Action<object> action, object p, Type type, bool processNow = false) {
            if (IsReady) {
                action(p);
            } else {
                _actionsToInvokeOnReady[type] = new TreeReadyAction() { Action = action, Parameter = p };
                if (processNow) {
                    TreeUpdateTask.ProcessPendingTextBufferChanges(async: true);
                }
            }
        }

        public IExpressionTermFilter ExpressionTermFilter { get; }
        #endregion

        /// <summary>
        /// True if tree is out of date and no longer matches current text buffer state
        /// </summary>
        public bool IsDirty => TreeUpdateTask.ChangesPending;

        #region Internal members
        /// <summary>
        /// Makes current thread owner of the tree.
        /// Normally only one thread can access the tree.
        /// </summary>
        internal void TakeThreadOwnerShip() {
            _ownerThread = Thread.CurrentThread.ManagedThreadId;
            TreeUpdateTask.TakeThreadOwnership();
            TreeLock.TakeThreadOwnership();
        }

        internal AstRoot GetAstRootUnsafe() => _astRoot;

        /// <summary>
        /// Async tree update task
        /// </summary>
        internal TreeUpdateTask TreeUpdateTask { get; private set; }
        #endregion

        #region Private members
        /// <summary>
        /// Tree lock
        /// </summary>
        internal EditorTreeLock TreeLock { get; private set; }

        /// <summary>
        /// Tree owner thread id (usually main thread)
        /// </summary>
        private int _ownerThread;

        /// <summary>
        /// Current text buffer snapshot
        /// </summary>
        private IEditorBufferSnapshot _textSnapShot;

        /// <summary>
        /// Parse tree
        /// </summary>
        private AstRoot _astRoot;

        class TreeReadyAction {
            public Action<object> Action;
            public object Parameter;
        }

        private readonly Dictionary<Type, TreeReadyAction> _actionsToInvokeOnReady = new Dictionary<Type, TreeReadyAction>();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates document tree on a given text buffer.
        /// </summary>
        /// <param name="editorBuffer">Editor buffer</param>
        /// <param name="services">Application services</param>
        public EditorTree(IEditorBuffer editorBuffer, IServiceContainer services) {
            _ownerThread = Thread.CurrentThread.ManagedThreadId;
            ExpressionTermFilter = editorBuffer.GetService<IExpressionTermFilter>();

            EditorBuffer = editorBuffer;
            EditorBuffer.ChangedHighPriority += OnTextBufferChanged;

            TreeUpdateTask = new TreeUpdateTask(this, services);
            TreeLock = new EditorTreeLock();
        }
        #endregion

        #region Building
        /// <summary>
        /// Builds initial AST. Subsequent updates should be coming from a background thread.
        /// </summary>
        public void Build() {
            Check.InvalidOperation(() => _ownerThread == Thread.CurrentThread.ManagedThreadId, NotMainThreadMessage);

            var sw = Stopwatch.StartNew();
            TreeUpdateTask.Cancel();

            if (EditorBuffer != null) {
                BufferSnapshot = EditorBuffer.CurrentSnapshot;
                _astRoot = RParser.Parse(EditorBuffer.CurrentSnapshot, ExpressionTermFilter);
            }

            TreeUpdateTask.ClearChanges();

            // Fire UpdatesPending notification, even though we don't have ranges for the event
            FireOnUpdatesPending();
            FireOnUpdateBegin();
            FireOnUpdateCompleted(TreeUpdateType.NewTree);

            sw.Stop();
        }

        /// <summary>
        /// Initiates processing of pending changes synchronously.
        /// </summary>
        internal void ProcessChanges() {
            if (IsDirty) {
                TreeUpdateTask.ProcessPendingTextBufferChanges(false);
            }
        }
        #endregion

        private void OnTextBufferChanged(object sender, TextChangeEventArgs e) => TreeUpdateTask.OnTextChange(e);

        internal void NotifyTextChange(int start, int oldLength, int newLength) 
            => _astRoot.ReflectTextChange(start, oldLength, newLength, EditorBuffer.CurrentSnapshot);

        public TreeTextChange PendingChanges => TreeUpdateTask.Changes;

        /// <summary>
        /// Removes nodes from the tree collection if node range is partially or entirely 
        /// within the deleted region. This is needed since parsing is asynchronous and 
        /// without node removal intellisense and syntax checker may end up processing
        /// nodes that are out of date. Where possible stops at the nearest scope level
        /// so scope nodes may still be used in smart indenter.
        /// </summary>
        /// <param name="range">Range to invalidate elements in</param>
        public bool InvalidateInRange(ITextRange range) {
            var removedElements = new List<IAstNode>();
            var firstToRemove = -1;
            var lastToRemove = -1;

            var node = AstRoot.NodeFromRange(range);
            var scope = node as IScope;
            while (scope == null || scope.OpenCurlyBrace == null || scope.CloseCurlyBrace == null ||
                    TextRange.Intersect(range, scope.OpenCurlyBrace) || TextRange.Intersect(range, scope.CloseCurlyBrace)) {
                scope = node.GetEnclosingScope();
                if (scope is GlobalScope) {
                    break;
                }
                node = scope;
            }

            for (var i = 0; i < scope.Children.Count; i++) {
                var child = scope.Children[i];
                if (range.Intersect(child)) {
                    if (firstToRemove < 0) {
                        firstToRemove = i;
                    } else {
                        lastToRemove = i;
                    }
                }
            }

            if (firstToRemove >= 0) {
                if (lastToRemove < 0) {
                    lastToRemove = firstToRemove;
                }
                for (var i = firstToRemove; i <= lastToRemove; i++) {
                    var child = scope.Children[i];
                    removedElements.Add(child);
                    _astRoot.Errors.RemoveInRange(child);
                }
                scope.RemoveChildren(firstToRemove, lastToRemove - firstToRemove + 1);
            }

            if (removedElements.Count > 0) {
                FireOnNodesRemoved(removedElements);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all elements from the tree
        /// </summary>
        /// <returns>Number of removed elements</returns>
        public void Invalidate() {
            // make sure not to use RootNode property since
            // calling get; causes parse
            var removedNodes = new List<IAstNode>();
            if (_astRoot.Children.Count > 0) {
                var gs = _astRoot.Children[0] as GlobalScope;
                removedNodes.AddRange(gs.Children);
                gs.RemoveChildren(0, gs.Children.Count);
            }

            if (removedNodes.Count > 0) {
                FireOnNodesRemoved(removedNodes);
            }
        }

        #region Tree Access
        public AstRoot AcquireReadLock(Guid treeUserId) {
            if (TreeLock != null) {
                if (TreeLock.AcquireReadLock(treeUserId)) {
                    return _astRoot;
                }
                Debug.Fail(Invariant($"Unable to acquire read lock for user {treeUserId}"));
            }
            return null;
        }

        public bool ReleaseReadLock(Guid treeUserId) => TreeLock.ReleaseReadLock(treeUserId);
        public bool AcquireWriteLock() => TreeLock.AcquireWriteLock();
        public bool ReleaseWriteLock() => TreeLock.ReleaseWriteLock();
        #endregion

        /// <summary>
        /// Determines position type and enclosing element node for a given position in the document text.
        /// </summary>
        /// <param name="position">Position in the document text</param>
        /// <param name="node">Node that contains position</param>
        /// <returns>Position type as a set of flags combined via OR operation</returns>
        public PositionType GetPositionElement(int position, out IAstNode node) => AstRoot.GetPositionNode(position, out node);

        #region Comments
        /// <summary>
        /// Determines if a given range is inside a comment and 
        /// returns range of the comment block.
        /// </summary>
        /// <param name="range">Text range to check</param>
        /// <returns>Text range of the found comment block or empty range if not found.</returns>
        public ITextRange GetCommentBlockContainingRange(ITextRange range) {
            TextRangeCollection<RToken> comments = this.AstRoot.Comments;

            var commentsInRange = comments.ItemsInRange(range);
            if (commentsInRange.Count == 1) {
                return commentsInRange[0];
            }

            return TextRange.EmptyRange;
        }

        /// <summary>
        /// Determines if a given position is inside a comment 
        /// and returns range of the comment block.
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Outer range of the found comment block or empty range if not found.</returns>
        public ITextRange GetCommentBlockFromPosition(int position) {
            return GetCommentBlockContainingRange(new TextRange(position, 0));
        }

        /// <summary>
        /// Determines if a given range is inside a comment.
        /// </summary>
        /// <param name="range">Text range</param>
        /// <returns>True if range is entirely inside a comment block</returns>
        public bool IsRangeInComment(ITextRange range) {
            var block = this.GetCommentBlockContainingRange(range);
            return block.Length > 0;
        }

        /// <summary>
        /// Determines if a given position is inside a comment of any kind.
        /// Comment may be plain HTML comment or artifact type comment
        /// like &lt;%--...--%> in ASP.NET or @* ... *@ in Razor.
        /// </summary>
        /// <param name="position">Position in the document</param>
        /// <returns>True if position is inside a comment block</returns>
        public bool IsRangeInComment(int position) {
            return IsRangeInComment(new TextRange(position, 0));
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (EditorBuffer != null) {
                    Closing?.Invoke(this, null);

                    TreeUpdateTask.Dispose();
                    TreeUpdateTask = null;

                    TreeLock?.Dispose();
                    TreeLock = null;

                    EditorBuffer.ChangedHighPriority -= OnTextBufferChanged;
                    EditorBuffer = null;
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public override string ToString()
            => Invariant($"IsDirty: {IsDirty} {TreeLock.ToString()} Changes: {TreeUpdateTask.ChangesPending}");
    }
}
