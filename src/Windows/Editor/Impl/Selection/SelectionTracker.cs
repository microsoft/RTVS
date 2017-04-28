// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.Languages.Editor.Selection {
    /// <summary>
    /// Selection tracker helps preserve caret position and relative screen scroll position 
    /// when text buffer change is applied. This is default implementation, languages can 
    /// choose to implement their own.
    /// </summary>
    public class SelectionTracker : ISelectionTracker {
        private readonly ITextView _textView;
        private double _offsetFromTop;
        private bool _automaticTracking;

        protected SnapshotPoint PositionBeforeChanges { get; set; }
        protected SnapshotPoint PositionAfterChanges { get; set; }
        protected int VirtualSpaces { get; set; }

        public SelectionTracker(IEditorView editorView) {
            EditorView = editorView;
            _textView = editorView.As<ITextView>();
        }

        #region ISelectionTracker
        /// <summary>
        /// Editor text view
        /// </summary>
        public IEditorView EditorView { get; }

        /// <summary>
        /// Saves current caret position and optionally starts tracking 
        /// of the caret position across text buffer changes using ITrackingSpan
        /// </summary>
        /// <param name="automaticTracking">True if selection tracker should track text buffer changes using tracking span. 
        /// False if End should simply use current caret position as final position rather than attempt to track it
        /// across changes.</param>
        public virtual void StartTracking(bool automaticTracking) {
            // After multiple changes calling EnsureVisible may randomly scroll the view since editor may no 
            // longer have an anchor position it can use and hence will simply make line caret is at last 
            // visible line. Thus we have to restore viewport position manually here.

            _automaticTracking = automaticTracking;

            // Beware: snapshot point may be relative to HTML buffer snapshot
            // and not relative to actual text view [projection] buffer snapshot.
            PositionBeforeChanges = _textView.Caret.Position.BufferPosition;
            PositionAfterChanges = PositionBeforeChanges;

            var viewLine = _textView.TextViewLines.GetTextViewLineContainingBufferPosition(PositionBeforeChanges);
            if (viewLine != null) {
                _offsetFromTop = viewLine.Top - _textView.ViewportTop;
            }
        }

        /// <summary>
        /// Stops tracking and saves current caret position
        /// as final position as final or 'after changes' position.
        /// </summary>
        public virtual void EndTracking() {
            if (_automaticTracking) {
                var snapshot = PositionBeforeChanges.Snapshot.TextBuffer.CurrentSnapshot;
                PositionAfterChanges = PositionBeforeChanges.TranslateTo(snapshot, PointTrackingMode.Positive);
            } else {
                PositionAfterChanges = _textView.Caret.Position.BufferPosition;
            }
            MoveToAfterChanges(VirtualSpaces);
        }

        /// <summary>
        /// Moves caret to 'before changes' position.
        /// </summary>
        public void MoveToBeforeChanges() => MoveCaretTo(PositionBeforeChanges, VirtualSpaces);

        /// <summary>
        /// Moves caret to 'after changes' position
        /// </summary>
        public void MoveToAfterChanges(int virtualSpaces = 0) => MoveCaretTo(PositionAfterChanges, virtualSpaces);
        #endregion

        protected virtual void MoveCaretTo(SnapshotPoint position, int virtualSpaces) {
            SnapshotPoint? viewPosition = _textView.MapUpToView(position);
            if (viewPosition.HasValue) {
                try {
                    // If position is in virtual space (virtualSpaces > 0) a new line
                    // was created as a result of smart indent. Upon Enter editor does
                    // not insert whitespace until user starts typing. In this case
                    // we want to offset from the start of the line rather than from
                    // the last know token position.
                    if (virtualSpaces > 0) {
                        var line = _textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(viewPosition.Value);
                        _textView.Caret.MoveTo(new VirtualSnapshotPoint(line, virtualSpaces));
                    } else {
                        _textView.Caret.MoveTo(viewPosition.Value);
                    }

                    if (_textView.Caret.ContainingTextViewLine.VisibilityState != VisibilityState.FullyVisible) {
                        _textView.Caret.EnsureVisible();
                        _textView.DisplayTextLineContainingBufferPosition(viewPosition.Value, _offsetFromTop, ViewRelativePosition.Top);
                    }
                } catch(ArgumentException) {
                    // In case formatting changes code in a way that position
                    // is no longer applicable. At worst caret position will move
                    // slightly which is better than popping up exception dialogs.
                }
            }
        }
    }
}
