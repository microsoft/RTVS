// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Core.AST;
using static System.FormattableString;

namespace Microsoft.R.Editor.Tree {
    public partial class EditorTree {
        private const string _threadCheckMessage = "Editor tree events must be fired on a main thread.";

        #region IEditorTree Events
        /// <summary>
        /// Event fires when there are text changes pending in the change queue.
        /// Tree users should stop using the tree and release read locks ASAP.
        /// Fires when user made changes to the text buffer and before initial
        /// tree nodes position updates.
        /// </summary>
        public event EventHandler<EventArgs> UpdatesPending;

        /// <summary>
        /// Signals that editor tree is about to be updated with the results
        /// of the background parsing.
        /// </summary>
        public event EventHandler<EventArgs> UpdateBegin;

        /// <summary>
        /// Fires when only node positions changed. No parsing was performed.
        /// </summary>
        public event EventHandler<TreePositionsOnlyChangedEventArgs> PositionsOnlyChanged;

        /// <summary>
        /// Fires when new elements were removed from the tree. Argument contains
        /// only top level removed elements. If listener is interested in all
        /// removed elements it needs to iterate over the subtree rooted at each 
        /// removed element.
        /// </summary>
        public event EventHandler<TreeNodesRemovedEventArgs> NodesRemoved;

        /// <summary>
        /// Fires when child elements of a scope node have changed. Typically
        /// means that relatively simple text editor was perfomed within { } 
        /// scope without generating changes in ancestor scope structure.
        /// </summary>
        //public event EventHandler<TreeScopeChangedEventArgs> ScopeChanged;

        /// <summary>
        /// Fires when editor tree update completes. Each change to the text buffer 
        /// produces one or two update calls. First call signals node position 
        /// updates and if tree is dirty (i.e. nodes changed) second call will follow 
        /// when asynchronous parsing is complete.
        /// </summary>
        public event EventHandler<TreeUpdatedEventArgs> UpdateCompleted;

        /// <summary>
        /// Fires when editor tree is closing. Listeners should finish
        /// aor cancel any outstanding processing and disconnect from 
        /// the tree events.
        /// </summary>
        public event EventHandler<EventArgs> Closing;
        #endregion

        /// <summary>
        /// Fires 'tree updates pending' event on the main thread context
        /// </summary>
        /// <param name="textChanges">List of pending changes</param>
        internal void FireOnUpdatesPending() {
            Check.InvalidOperation(() => Thread.CurrentThread.ManagedThreadId == _ownerThread, _threadCheckMessage);
            try {
                UpdatesPending?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                Debug.Assert(false, Invariant($"Exception thrown in a tree.UpdatesPending event handler: {ex.Message}"));
            }
        }

        /// <summary>
        /// Fires 'positions changed starting inside this element' event on the main thread context
        /// </summary>
        /// <param name="element">Element</param>
        internal void FireOnPositionsOnlyChanged() {
            Check.InvalidOperation(() => Thread.CurrentThread.ManagedThreadId == _ownerThread, _threadCheckMessage);
            try {
                PositionsOnlyChanged?.Invoke(this, new TreePositionsOnlyChangedEventArgs());
            } catch (Exception ex) {
                Debug.Assert(false, Invariant($"Exception thrown in a tree.PositionsOnlyChanged event handler: {ex.Message}"));
            }
        }

        /// <summary>
        /// Fires 'nodes removed' event on the main thread context
        /// </summary>
        /// <param name="nodes">Collection of removed nodes</param>
        internal void FireOnNodesRemoved(IReadOnlyCollection<IAstNode> nodes) {
            Check.InvalidOperation(() => Thread.CurrentThread.ManagedThreadId == _ownerThread, _threadCheckMessage);
            try {
                // Don't bother if list is empty
                if (nodes.Count > 0) {
                    NodesRemoved?.Invoke(this, new TreeNodesRemovedEventArgs(nodes));
                }
            } catch (Exception ex) {
                Debug.Assert(false, Invariant($"Exception thrown in a tree.NodesRemoved event handler: {ex.Message}"));
            }
        }

        /// <summary>
        /// Fires 'update begin' event on the main thread context
        /// </summary>
        internal void FireOnUpdateBegin() {
            Check.InvalidOperation(() => Thread.CurrentThread.ManagedThreadId == _ownerThread, _threadCheckMessage);
            try {
                UpdateBegin?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                Debug.Assert(false, Invariant($"Exception thrown in a tree.UpdateBegin event handler: {ex.Message}"));
            }
        }

        /// <summary>
        /// Fires 'update end' event on the main thread context
        /// </summary>
        internal void FireOnUpdateCompleted(TreeUpdateType updateType) {
            Check.InvalidOperation(() => Thread.CurrentThread.ManagedThreadId == _ownerThread, _threadCheckMessage);
            try {
                UpdateCompleted?.Invoke(this, new TreeUpdatedEventArgs(updateType));
            } catch (Exception ex) {
                Debug.Assert(false, Invariant($"Exception thrown in a tree.UpdateCompleted event handler: {ex.Message}"));
            }
        }
    }
}
