// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Tree;

namespace Microsoft.R.Editor.Validation.Tagger {
    /// <summary>
    /// Represents collection of HTML validation errors and tasks. Tag collection
    /// is a primary source for validation squiggles in the editor as well as
    /// source of the corresponding messages for the application task list.
    /// Collection is thread safe.
    /// </summary>
    internal sealed class ErrorTagCollection {
        /// <summary>
        /// Queue of tags removed in async operations. Typically used
        /// by task list item sources to update the task list.
        /// </summary>
        public ConcurrentQueue<EditorErrorTag> RemovedTags { get; private set; }

        /// <summary>
        /// Collection lock. Collection does permin access from multiple threads.
        /// </summary>
        private readonly object _lockObj = new object();

        /// <summary>
        /// Collection of error tags. Each tag is also a text range in the document.
        /// </summary>
        private TextRangeCollection<EditorErrorTag> _tags = new TextRangeCollection<EditorErrorTag>();

        /// <summary>
        /// Queue of element keys representing nodes which associated
        /// error tags need to be removed from the collection. Removal 
        /// is performed in a separate thread.
        /// </summary>
        private ConcurrentQueue<IAstNode> _nodesPendingRemoval = new ConcurrentQueue<IAstNode>();

        /// <summary>
        /// Compound text range that encloses added and removed tags. 
        /// Calculated for changes that happen between BeginUpdate 
        /// and EndUpdate calls.
        /// </summary>
        private ITextRange _removedRange;

        /// <summary>
        /// Editor tree
        /// </summary>
        private IREditorTree _editorTree;

        /// <summary>
        /// Indicates if background task that is removing tags for 
        /// deleted elements is currently running.
        /// </summary>
        private long _taskRunning = 0;

        public ErrorTagCollection(IREditorTree editorTree) {
            RemovedTags = new ConcurrentQueue<EditorErrorTag>();

            _editorTree = editorTree;

            _editorTree.Closing += OnTreeClosing;
            _editorTree.NodesRemoved += OnNodesRemoved;
            _editorTree.UpdateCompleted += OnUpdateCompleted;
        }

        public EditorErrorTag[] ToArray() {
            EditorErrorTag[] array;

            lock (_lockObj) {
                array = _tags.ToArray();
            }

            return array;

        }

        public IReadOnlyList<EditorErrorTag> ItemsInRange(ITextRange range) {
            IReadOnlyList<EditorErrorTag> list;

            lock (_lockObj) {
                list = _tags.ItemsInRange(range);
            }

            return list;
        }

        public void Clear() {
            lock (_lockObj) {
                foreach (var tag in _tags) {
                    RemovedTags.Enqueue(tag);
                }

                _tags.Clear();
            }
        }

        public void ReflectTextChange(int start, int oldLength, int newLength, bool trivialChange) {
            lock (_lockObj) {
                // Remove items first. This is different from default ReflectTextChange
                // implementation since ReflectTextChange does not remove item
                // that fully contains the change and rather expands it. We don't 
                // want this behavior since changed elementneeds to be revalidated
                // and hence expanding squiggle does not make sense.
                ICollection<EditorErrorTag> removed = null;
                if (!trivialChange) {
                    removed = _tags.RemoveInRange(new TextRange(start, oldLength), true);
                }

                _tags.ReflectTextChange(start, oldLength, newLength);

                if (!trivialChange) {
                    foreach (var tag in removed) {
                        RemovedTags.Enqueue(tag);
                    }
                }
            }
        }

        internal void Add(EditorErrorTag tag) {
            lock (_lockObj) {
                _tags.Add(tag);
            }
        }

        /// <summary>
        /// Signals that collection is about to be updated. Must be called
        /// before any updates are made to the collection so collection can
        /// calculated span of changes in the text buffer.
        /// </summary>
        /// <returns></returns>
        public ITextRange BeginUpdate() {
            ITextRange range = TextRange.EmptyRange;

            if (_editorTree != null) {
                SpinWait.SpinUntil(() => Interlocked.Read(ref _taskRunning) == 0);

                lock (_lockObj) {
                    if (_removedRange != null) {
                        // Removed range may be out of current snapshot boundaries if, say, 
                        // a lot of elements were removed. Clip it appropriately.

                        range = TextRange.Intersection(_removedRange, 0, _editorTree.TextBuffer().CurrentSnapshot.Length);
                    }

                    _removedRange = null;
                }
            }

            return range;
        }

        /// <summary>
        /// Signals that collection update has been completed.
        /// </summary>
        /// <param name="modified">True if collection was indeed modified since BeginUpdate was called</param>
        public void EndUpdate(bool modified) {
            if (modified) {
                lock (_lockObj) {
                    _tags.Sort();
                }
            }
        }

        /// <summary>
        /// Removes all tags associated with a given node
        /// </summary>
        /// <param name="node">Node in the AST</param>
        /// <returns>Text range that encloses removed tag spans</returns>
        public ITextRange RemoveTagsForNode(IAstNode node) {
            int start = _editorTree.TextBuffer().CurrentSnapshot.Length;
            ITextRange range = TextRange.EmptyRange;
            int end = 0;

            lock (_lockObj) {
                // Remove all tags for this node
                for (int i = 0; i < _tags.Count; i++) {
                    if (TextRange.ContainsInclusiveEnd(node, _tags[i])) {
                        start = Math.Min(start, _tags[i].Start);
                        end = Math.Max(end, _tags[i].End);

                        RemovedTags.Enqueue(_tags[i]);

                        _tags.RemoveAt(i);
                        i--;

                        range = TextRange.Union(range, start, end - start);
                    }
                }
            }

            return range;
        }

        private void OnTreeClosing(object sender, EventArgs e) {
            if (_editorTree != null) {
                _editorTree.Closing -= OnTreeClosing;
                _editorTree.NodesRemoved -= OnNodesRemoved;
                _editorTree.UpdateCompleted -= OnUpdateCompleted;

                _editorTree = null;
            }
        }

        private void OnNewTree(object sender, EventArgs e) {
            Clear();
        }

        private void OnNodesRemoved(object sender, TreeNodesRemovedEventArgs e) {
            foreach (var node in e.Nodes) {
                StoreRemovedNodes(node);
            }
        }

        private void OnUpdateCompleted(object sender, TreeUpdatedEventArgs e) {
            if (_nodesPendingRemoval.Count > 0) {
                if (Interlocked.CompareExchange(ref _taskRunning, 1, 0) == 0) {
                    Task.Run(() => ProcessPendingNodeRemoval());
                }
            }
        }

        private void StoreRemovedNodes(IAstNode node) {
            foreach (var child in node.Children) {
                StoreRemovedNodes(child);
            }

            _nodesPendingRemoval.Enqueue(node);
        }

        private void ProcessPendingNodeRemoval() {
            int start = Int32.MaxValue;
            int end = 0;

            try {
                if (_nodesPendingRemoval.Count > 0) {
                    lock (_lockObj) {
                        IAstNode node;

                        while (_nodesPendingRemoval.TryDequeue(out node)) {
                            for (int j = 0; j < _tags.Count; j++) {
                                if (TextRange.ContainsInclusiveEnd(node, _tags[j]) || node.Parent == null) {
                                    start = Math.Min(start, _tags[j].Start);
                                    end = Math.Max(end, _tags[j].End);

                                    RemovedTags.Enqueue(_tags[j]);

                                    _tags.RemoveAt(j);
                                    j--;
                                }
                            }
                        }

                        if (start != Int32.MaxValue) {
                            if (_removedRange != null) {
                                _removedRange = TextRange.Union(_removedRange, start, end - start);
                            } else {
                                _removedRange = TextRange.FromBounds(start, end);
                            }
                        }
                    }
                }
            } finally {
                Interlocked.Exchange(ref _taskRunning, 0);
            }
        }
    }
}
