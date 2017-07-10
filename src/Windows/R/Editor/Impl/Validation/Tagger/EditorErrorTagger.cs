// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Validation.Tagger {
    /// <summary>
    /// This gives the editor a list of ranges that need to be underlined with red squiggles.
    /// As the stylesheet changes, this class keeps the list of syntax errors up to date.
    /// </summary>
    public class EditorErrorTagger : ITagger<IErrorTag>, IEditorTaskListItemSource {
        private readonly IEditorTaskList _taskList;
        private readonly IREditorSettings _settings;
        private readonly IIdleTimeService _idleTime;
        private readonly ITextBuffer _textBuffer;
        private readonly ErrorTagCollection _errorTags;
        private readonly IREditorDocument _document;
        private readonly ConcurrentQueue<IValidationError> _resultsQueue;


        public EditorErrorTagger(ITextBuffer textBuffer, IEditorTaskList taskList, IServiceContainer services) {
            _taskList = taskList;

            _settings = services.GetService<IREditorSettings>();
            _idleTime = services.GetService<IIdleTimeService>();

            _document = textBuffer.GetEditorDocument<IREditorDocument>();
            _document.Closing += OnDocumentClosing;
            _document.EditorTree.UpdateCompleted += OnTreeUpdateCompleted;
            _document.EditorTree.NodesRemoved += OnNodesRemoved;
            _errorTags = new ErrorTagCollection(_document.EditorTree);

            _textBuffer = _document.EditorTree.TextBuffer();
            _textBuffer.Changed += OnTextBufferChanged;

            // Don't push syntax errors to the Error List in transient
            // documents such as in document attached to a projected buffer
            // in the R interactive window
            if (_taskList != null) {
                var view = _document.PrimaryView;
                if (view != null && !view.IsRepl()) {
                    _taskList.AddTaskSource(this);
                }
            }

            var validator = _document.EditorBuffer.GetService<TreeValidator>();
            validator.Cleared += OnCleared;

            _resultsQueue = validator.ValidationResults;
            _idleTime.Idle += OnIdle;
        }

        /// <summary>
        /// Tells the editor (or any listener) that syntax errors have changed
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Retriever error (squiggly) tagger associated with the text buffer
        /// </summary>
        /// <param name="textBuffer">Text buffer</param>
        public static EditorErrorTagger FromTextBuffer(ITextBuffer textBuffer)
            => textBuffer.GetService<EditorErrorTagger>();

        private void OnNodesRemoved(object sender, TreeNodesRemovedEventArgs e) {
            foreach (var node in e.Nodes) {
                _errorTags.RemoveTagsForNode(node);
            }
        }

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e) {
            if (e.UpdateType != TreeUpdateType.PositionsOnly || _settings.LintOptions.Enabled) {
                RemoveAllTags();
            }
        }

        private void OnCleared(object sender, EventArgs e) => RemoveAllTags();

        private void RemoveAllTags() {
            _errorTags.Clear();

            TasksCleared?.Invoke(this, EventArgs.Empty);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
                                new SnapshotSpan(_textBuffer.CurrentSnapshot, 0, _textBuffer.CurrentSnapshot.Length)));
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            if (_settings.SyntaxCheckEnabled && e.Changes.Count > 0) {
                var change = e.ToTextChange();
                _errorTags.ReflectTextChange(change.Start, change.OldLength, change.NewLength,
                                             trivialChange: !_document.EditorTree.IsReady);

                if (_errorTags.RemovedTags.Count > 0 && TagsChanged != null) {
                    var start = int.MaxValue;
                    var end = int.MinValue;

                    foreach (var errorTag in _errorTags.RemovedTags) {
                        start = Math.Min(start, errorTag.Start);
                        end = Math.Max(end, errorTag.End);
                    }

                    // RemovedTags haven't had their positions updated, verify their 
                    //   values won't break the SnapshotSpan creation
                    var snapshot = _textBuffer.CurrentSnapshot;
                    start = Math.Min(start, snapshot.Length);
                    end = Math.Min(end, snapshot.Length);

                    TagsChanged(this, new SnapshotSpanEventArgs(
                                        new SnapshotSpan(snapshot, start, end - start)));
                }

                TasksUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Tree event handlers
        private void OnDocumentClosing(object sender, EventArgs e) {
            if (_textBuffer != null) {
                _idleTime.Idle -= OnIdle;

                _document.EditorTree.UpdateCompleted -= OnTreeUpdateCompleted;
                _document.EditorTree.NodesRemoved -= OnNodesRemoved;
                _document.Closing -= OnDocumentClosing;

                _errorTags.Clear();

                _textBuffer.Changed -= OnTextBufferChanged;
                _taskList?.RemoveTaskSource(this);
            }
        }
        #endregion

        /// <summary>
        /// Idle time event handler. Retrieves results from the validation task queue,
        /// creates new error tags (squiggles) and fires an event telling editor that
        /// tags changed. The code must operate on UI thread and hence it is called on idle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnIdle(object sender, EventArgs eventArgs) {
            if (_settings.SyntaxCheckEnabled && _textBuffer != null) {
                if (_resultsQueue.Count > 0) {

                    var addedTags = new List<IEditorTaskListItem>();
                    var changedRange = _errorTags.BeginUpdate();
                    changedRange = TextRange.Intersection(changedRange, 0, _textBuffer.CurrentSnapshot.Length);

                    var timer = Stopwatch.StartNew();
                    timer.Reset();

                    while (timer.ElapsedMilliseconds < 100) {
                        if (!_resultsQueue.TryDequeue(out var error)) {
                            break;
                        }

                        if (string.IsNullOrEmpty(error.Message)) {
                            // Empty message means remove all errors.
                            changedRange = new TextRange(0, _textBuffer.CurrentSnapshot.Length);
                        } else {
                            var tag = new EditorErrorTag(_document.EditorTree, error);
                            if (tag.Length > 0) {
                                if (changedRange.End == 0) {
                                    changedRange = tag;
                                } else {
                                    changedRange = changedRange.Union(tag);
                                }

                                _errorTags.Add(tag);
                                addedTags.Add(tag);
                            }
                        }
                    }

                    _errorTags.EndUpdate(changedRange.Length > 0);

                    // Clip range to the current snapshot
                    var start = Math.Max(changedRange.Start, 0);
                    start = Math.Min(start, _textBuffer.CurrentSnapshot.Length);
                    var end = Math.Min(changedRange.End, _textBuffer.CurrentSnapshot.Length);

                    if (changedRange.Length > 0) {
                        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, start, end - start)));
                    }

                    BeginUpdatingTasks?.Invoke(this, EventArgs.Empty);

                    try {
                        if (addedTags.Count > 0) {
                            TasksAdded?.Invoke(this, new TasksListItemsChangedEventArgs(addedTags));
                        }
                        if (_errorTags.RemovedTags.Count > 0) {

                            var removedTags = new List<IEditorTaskListItem>();
                            while (_errorTags.RemovedTags.Count > 0) {
                                if (_errorTags.RemovedTags.TryDequeue(out var tag)) {
                                    removedTags.Add(tag);
                                }
                            }
                            if (removedTags.Count > 0) {
                                TasksRemoved?.Invoke(this, new TasksListItemsChangedEventArgs(removedTags));
                            }
                        }
                    } finally {
                        EndUpdatingTasks?.Invoke(this, EventArgs.Empty);
                    }

                    timer.Stop();
                }
            }
        }

        /// <summary>
        /// Provides the list of squiggles to the editor
        /// </summary>
        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            var tags = new List<ITagSpan<IErrorTag>>();

            if (_settings.SyntaxCheckEnabled && _errorTags != null) {
                foreach (var span in spans) {
                    // Force the input span to cover at least one character
                    // (this helps make tooltips work where the input span is empty)
                    var spanAfterEnd = (span.Length == 0) ? span.Start.Position + 1 : span.End.Position;
                    tags.AddRange(_errorTags.ItemsInRange(TextRange.FromBounds(span.Start, spanAfterEnd)));
                }
            }

            return tags;
        }

        #region IEditorTaskListItemSource

        public event EventHandler<TasksListItemsChangedEventArgs> TasksAdded;
        public event EventHandler<TasksListItemsChangedEventArgs> TasksRemoved;
        public event EventHandler<EventArgs> BeginUpdatingTasks;
        public event EventHandler<EventArgs> EndUpdatingTasks;
#pragma warning disable 67
        public event EventHandler<EventArgs> TasksCleared;
#pragma warning restore 67
        public event EventHandler<EventArgs> TasksUpdated;

        public IReadOnlyCollection<IEditorTaskListItem> Tasks {
            get {
                var array = _errorTags.ToArray();
                var tasks = new List<IEditorTaskListItem>();

                tasks.AddRange(array);
                return tasks;
            }
        }
        public object EditorBuffer => _textBuffer;
        #endregion
    }
}
