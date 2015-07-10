using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Tree
{
    public partial class EditorTree
    {
        private const string _threadContextInvalidMessage = "R editor tree events must be fired on a main thread.";

        #region Events
        /// <summary>
        /// Event fires when there are text changes pending in the change queue.
        /// Tree users should stop using the tree and release read locks ASAP.
        /// Called when user made changes to the text buffer and before initial
        /// tree nodes position updates.
        /// </summary>
        public event EventHandler<TreeUpdatePendingEventArgs> UpdatesPending;

        /// <summary>
        /// Event fires when all updates have been processed.
        /// </summary>
        public event EventHandler<EventArgs> UpdatesCompleted;

        /// <summary>
        /// Signals that editor tree is about to be updated
        /// </summary>
        public event EventHandler<EventArgs> UpdateBegin;

        /// <summary>
        /// Fires when only node positions changed
        /// </summary>
        public event EventHandler<TreePositionsOnlyChangedEventArgs> PositionsOnlyChanged;

        /// <summary>
        /// Fires when only node positions changed
        /// </summary>
        public event EventHandler<TreeTokenNodeChangedEventArgs> TokenNodeChanged;

        /// <summary>
        /// Fires when new elements were removed from the tree. Argument contains
        /// only top level removed elements. If listener is interested in all
        /// removed elements it needs to iterate over the subtree rooted at each 
        /// removed element.
        /// </summary>
        public event EventHandler<TreeNodesRemovedEventArgs> NodesRemoved;

        /// <summary>
        /// Fires when full document reparse is complete and new tree was created.
        /// </summary>
        public event EventHandler<EventArgs> NewTree;

        /// <summary>
        /// Fires when editor tree update completes. Each change to the text buffer
        /// produces one or two update calls. First call signals node position updates
        /// and if tree is dirty (i.e. nodes changed) second call will follow when
        /// asynchronous parsing is complete.
        /// </summary>
        public event EventHandler<TreeUpdatedEventArgs> UpdateCompletedHighPriority;

        /// <summary>
        /// Fires when editor tree update completes and after <seealso cref="UpdateCompletedHighPriority"/>. 
        /// was fired. Each change to the text buffer produces one or two update calls. First call signals 
        /// node position updates and if tree is dirty (i.e. nodes changed) second call will follow 
        /// when asynchronous parsing is complete.
        /// </summary>
        public event EventHandler<TreeUpdatedEventArgs> UpdateCompleted;

        /// <summary>
        /// Fires when editor tree update completes and after <seealso cref="UpdateCompleted"/>. 
        /// was fired. Each change to the text buffer produces one or two update calls. First call signals 
        /// node position updates and if tree is dirty (i.e. nodes changed) second call will follow 
        /// when asynchronous parsing is complete.
        /// </summary>
        public event EventHandler<TreeUpdatedEventArgs> UpdateCompletedLowPriority;

        /// <summary>
        /// Fires when editor tree is closing.
        /// </summary>
        public event EventHandler<EventArgs> Closing;
        #endregion

        /// <summary>
        /// Fires 'tree updates pending' event on the main thread context
        /// </summary>
        /// <param name="textChanges">List of pending changes</param>
        internal void FireOnUpdatesPending(IReadOnlyCollection<TextChangeEventArgs> textChanges)
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                if (UpdatesPending != null)
                    UpdatesPending(this, new TreeUpdatePendingEventArgs(textChanges));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.OnUpdatesPending event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'tree updates completed' event on the main thread context
        /// </summary>
        internal void FireOnUpdatesCompleted()
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                if (UpdatesCompleted != null)
                    UpdatesCompleted(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.OnUpdatesCompleted event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'positions changed starting inside this element' event on the main thread context
        /// </summary>
        /// <param name="element">Element</param>
        internal void FireOnPositionsOnlyChanged(IAstNode node)
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                if (PositionsOnlyChanged != null)
                    PositionsOnlyChanged(this, new TreePositionsOnlyChangedEventArgs(node.Key));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.OnPositionsOnlyChanged event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'nodes removed' event on the main thread context
        /// </summary>
        /// <param name="nodes">Collection of removed nodes</param>
        internal void FireOnNodesRemoved(IReadOnlyCollection<IAstNode> nodes)
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                // Don't bother if list is empty
                if (nodes.Count > 0 && NodesRemoved != null)
                    NodesRemoved(this, new TreeNodesRemovedEventArgs(nodes));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.FireOnElementsRemoved event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'token node changed' event on the main thread context
        /// </summary>
        internal void FireOnTokenNodeChanged(IAstNode node)
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                if (TokenNodeChanged != null)
                    TokenNodeChanged(this, new TreeTokenNodeChangedEventArgs(node.Key));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.FireOnTokenNodeChanged event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'update begin' event on the main thread context
        /// </summary>
        internal void FireOnUpdateBegin()
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                if (UpdateBegin != null)
                    UpdateBegin(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.OnUpdateBegin event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'new tree' event on the main thread context
        /// </summary>
        internal void FireOnNewTree()
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            try
            {
                if (NewTree != null)
                    NewTree(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format(CultureInfo.CurrentCulture,
                    "Exception thrown in a tree.OnNewTree event handler: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fires 'update end' event on the main thread context
        /// </summary>
        internal void FireOnUpdateCompleted(TreeUpdateType updateType, bool fullParse)
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Fail(_threadContextInvalidMessage);
                return;
            }

            if (UpdateCompletedHighPriority != null)
                UpdateCompletedHighPriority(this, new TreeUpdatedEventArgs(updateType, fullParse));

            if (UpdateCompleted != null)
                UpdateCompleted(this, new TreeUpdatedEventArgs(updateType, fullParse));

            if (UpdateCompletedLowPriority != null)
                UpdateCompletedLowPriority(this, new TreeUpdatedEventArgs(updateType, fullParse));
        }
    }
}
