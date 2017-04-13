// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Editor document parse tree (object model)
    /// </summary>
    public interface IREditorTree: IDisposable {
        /// <summary>
        /// Document text buffer
        /// </summary>
        IEditorBuffer EditorBuffer { get; }

        /// <summary>
        /// Last text snapshot associated with this tree
        /// </summary>
        IEditorBufferSnapshot BufferSnapshot { get; }

        /// <summary>
        /// True if tree matches current text buffer snapshot.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Parsed document. Since change processing is asynchronous,
        /// the AST maybe out of date relative to the current text 
        /// buffer content
        /// </summary>
        AstRoot AstRoot { get; }

        /// <summary>
        /// Event fires when there are text changes pending in the change queue.
        /// Tree users should stop using the tree and release read locks ASAP.
        /// Fires when user made changes to the text buffer and before initial
        /// tree nodes position updates.
        /// </summary>
        event EventHandler<EventArgs> UpdatesPending;

        /// <summary>
        /// Signals that editor tree is about to be updated with the results
        /// of the background parsing.
        /// </summary>
        event EventHandler<EventArgs> UpdateBegin;

        /// <summary>
        /// Fires when only node positions changed. No parsing was performed.
        /// </summary>
        event EventHandler<TreePositionsOnlyChangedEventArgs> PositionsOnlyChanged;

        /// <summary>
        /// Fires when new elements were removed from the tree. Argument contains
        /// only top level removed elements. If listener is interested in all
        /// removed elements it needs to iterate over the subtree rooted at each 
        /// removed element.
        /// </summary>
        event EventHandler<TreeNodesRemovedEventArgs> NodesRemoved;

        /// <summary>
        /// Fires when child elements of a scope node have changed. Typically
        /// means that relatively simple text editor was perfomed within { } 
        /// scope without generating changes in ancestor scope structure.
        /// </summary>
        //event EventHandler<TreeScopeChangedEventArgs> ScopeChanged;

        /// <summary>
        /// Fires when editor tree update completes. Each change to the text buffer 
        /// produces one or two update calls. First call signals node position 
        /// updates and if tree is dirty (i.e. nodes changed) second call will follow 
        /// when asynchronous parsing is complete.
        /// </summary>
        event EventHandler<TreeUpdatedEventArgs> UpdateCompleted;

        /// <summary>
        /// Fires when editor tree is closing. Listeners should finish
        /// aor cancel any outstanding processing and disconnect from 
        /// the tree events.
        /// </summary>
        event EventHandler<EventArgs> Closing;

        /// <summary>
        /// Acquired read lock on the tree. The method is intended to be used
        /// from background threads. UI thread doesn't have to acquire locks
        /// to access the tree. If tree reader pusage of the tree may be long
        /// it should listen to 'update pending' events and release tree lock
        /// promptly letting editor to update the tree with the changes made
        /// to the text buffer.
        /// </summary>
        /// <param name="treeUserId">Unique identifier of the tree user</param>
        /// <returns>AST root if lock was acquired</returns>
        AstRoot AcquireReadLock(Guid treeUserId);

        /// <summary>
        /// Releases previously acquired tree read lock.
        /// </summary>
        /// <param name="treeUserId"></param>
        /// <returns>True if lock was released</returns>
        bool ReleaseReadLock(Guid treeUserId);

        /// <summary>
        /// Invalidates the entire tree. Typically used before massive
        /// text replace operations in order to simplify life
        /// of the background parser
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Ensures tree is up to date, matches current text buffer snapshot
        /// and all changes since the last update were processed. Blocks until 
        /// all changes have been processed. Does not pump Windows messages.
        /// Warning: calling this method frequently or on a critical performance 
        /// path (such as during typing) is highly not recommended.
        /// </summary>
        void EnsureTreeReady();

        /// <summary>
        /// Provides a way to automatically invoke particular action
        /// once when tree becomes ready again. Typically used in
        /// asynchronous completion and signature help scenarios.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="p">Parameter to pass to the action</param>
        /// <param name="type">Action identifier</param>
        void InvokeWhenReady(Action<object> action, object p, Type type, bool processNow = false);

        /// <summary>
        /// Allows primary language to filter expression terms
        /// </summary>
        IExpressionTermFilter ExpressionTermFilter { get; }
    }
}
