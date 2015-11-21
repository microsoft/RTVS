using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.R.Package.History {
    internal class HistoryWindowPaneMouseProcessor : MouseProcessorBase, IMouseProcessor2 {
        private readonly IWpfTextView _textView;
        //private readonly ITextStructureNavigatorSelectorService _textStructureNavigatorProvider;
        //private readonly IEditorOperations _editorOperations;
        private readonly IRHistory _history;

        //private ITrackingSpan _originalSelectedWord;
        //private ITrackingSpan _originalSelectedLine;
        //private bool _doingWordSelection;
        //private bool _doingLineSelection;

        private TimeSpan _elapsedSinceLastTap;
        private Point _lastTapPosition;
        private Point _currentTapPosition;

        private readonly Stopwatch _doubleTapStopWatch = new Stopwatch();
        private readonly TimeSpan _maximumElapsedDoubleTap = new TimeSpan(0, 0, 0, 0, 600);
        private readonly int _minimumPositionDelta = 30;
        //private readonly CountdownDisposable _ignoreSelectionChangedEvents = new CountdownDisposable();

        public HistoryWindowPaneMouseProcessor(IWpfTextView wpfTextView, IRHistoryProvider historyProvider) {

            _textView = wpfTextView;
            //_textStructureNavigatorProvider = textStructureNavigatorProvider;
            _history = historyProvider.GetAssociatedRHistory(_textView);

            _textView.Selection.SelectionChanged += SelectionChanged;
            _textView.Closed += TextViewClosed;

            //_originalSelectedWord = null;
            //_originalSelectedLine = null;
            //_editorOperations = editorOperationsProvider.GetEditorOperations(wpfTextView);
        }

        private void TextViewClosed(object sender, EventArgs e) {
            _textView.Selection.SelectionChanged -= SelectionChanged;
            _textView.Closed -= TextViewClosed;
        }

        private void SelectionChanged(object sender, EventArgs args) {
            //if (_ignoreSelectionChangedEvents.Count == 0) {
            //    _doingWordSelection = false;
            //    _doingLineSelection = false;
            //    _originalSelectedWord = null;
            //    _originalSelectedLine = null;

                
            //}

            if (_textView.Selection.Start != _textView.Selection.End) {
                _history.ClearHistoryEntrySelection();
            }
        }

        #region IMouseProcessorProvider Member Implementations

        public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e) {
            HandleLeftButtonDown(e);
        }

        /// <summary>
        /// Handles the Mouse Move event before the default handler 
        /// </summary>
        //public override void PreprocessMouseMove(MouseEventArgs e) {
        //    if (e == null) {
        //        throw new ArgumentNullException(nameof(e));
        //    }

        //    e.Handled = PreprocessMouseMoveByPosition(GetAdjustedPosition(e, _textView), e.LeftButton);
        //}

        /// <summary>
        /// Handles the Mouse up event
        /// </summary>
        public override void PostprocessMouseUp(MouseButtonEventArgs e) {
            //_doingLineSelection = false;
            //_doingWordSelection = false;
            //_originalSelectedLine = null;
            //_originalSelectedWord = null;

            _lastTapPosition = GetAdjustedPosition(e, _textView);
            _doubleTapStopWatch.Restart();
        }

        public void PreprocessTouchDown(TouchEventArgs e) {
            _currentTapPosition = GetAdjustedPosition(e, _textView);
            _elapsedSinceLastTap = _doubleTapStopWatch.Elapsed;
            _doubleTapStopWatch.Restart();

            HandleLeftButtonDown(e);

            _lastTapPosition = _currentTapPosition;
        }

        public void PostprocessTouchDown(TouchEventArgs e) { }

        public void PreprocessTouchUp(TouchEventArgs e) { }

        public void PostprocessTouchUp(TouchEventArgs e) {
            //_doingLineSelection = false;
            //_doingWordSelection = false;
            //_originalSelectedLine = null;
            //_originalSelectedWord = null;
        }

        public void PreprocessManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e) { }

        public void PostprocessManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e) { }

        public void PreprocessManipulationStarting(ManipulationStartingEventArgs e) { }

        public void PostprocessManipulationStarting(ManipulationStartingEventArgs e) { }

        public void PreprocessManipulationDelta(ManipulationDeltaEventArgs e) { }

        public void PostprocessManipulationDelta(ManipulationDeltaEventArgs e) { }

        public void PreprocessManipulationCompleted(ManipulationCompletedEventArgs e) { }

        public void PostprocessManipulationCompleted(ManipulationCompletedEventArgs e) { }

        public void PreprocessStylusSystemGesture(StylusSystemGestureEventArgs e) { }

        public void PostprocessStylusSystemGesture(StylusSystemGestureEventArgs e) { }

        #endregion

        private void HandleLeftButtonDown(InputEventArgs e) {
            if (e == null) {
                throw new ArgumentNullException(nameof(e));
            }

            var clickCount = GetClickCount(e);
            var modifiers = (Keyboard.Modifiers & ModifierKeys.Shift) | (Keyboard.Modifiers & ModifierKeys.Control);

            switch (clickCount) {
                case 1:
                    e.Handled = HandleSingleClick(e, modifiers);
                    break;
                case 2:
                    e.Handled = HandleDoubleClick(e, modifiers);
                    break;
                case 3:
                    // Disable triple click
                    e.Handled = true;
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        private static Point GetAdjustedPosition(InputEventArgs e, IWpfTextView view) {
            var pt = GetPosition(e, view.VisualElement);

            pt.X += view.ViewportLeft;
            pt.Y += view.ViewportTop;

            return pt;
        }

        private static Point GetPosition(InputEventArgs e, FrameworkElement fe) {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null) {
                return mouseEventArgs.GetPosition(fe);
            }

            var touchEventArgs = e as TouchEventArgs;
            if (touchEventArgs != null) {
                return touchEventArgs.GetTouchPoint(fe).Position;
            }

            return new Point(0, 0);
        }

        private int GetClickCount(InputEventArgs e) {
            var clickCount = 1;
            var mouseButtonEventArgs = e as MouseButtonEventArgs;
            if (mouseButtonEventArgs != null) {
                return mouseButtonEventArgs.ClickCount;
            }

            if (e is TouchEventArgs) {
                clickCount = 1;
                bool tapsAreCloseTogether = (Math.Abs(_currentTapPosition.X - _lastTapPosition.X) < _minimumPositionDelta) && (Math.Abs(_currentTapPosition.Y - _lastTapPosition.Y) < _minimumPositionDelta);
                bool tapsAreCloseInTime = (_elapsedSinceLastTap != TimeSpan.Zero) && _elapsedSinceLastTap < _maximumElapsedDoubleTap;

                if (tapsAreCloseInTime && tapsAreCloseTogether) {
                    // treat as a double tap
                    clickCount = 2;
                }
            }

            return clickCount;
        }

        private bool HandleSingleClick(InputEventArgs e, ModifierKeys modifiers) {
            // Don't do anything if there is no history
            if (_textView.TextBuffer.CurrentSnapshot.Length == 0) {
                return true;
            }

            var point = GetAdjustedPosition(e, _textView);
            var lineNumber = GetLineNumberUnderPoint(point);
            if (lineNumber == -1) {
                return false;
            }

            switch (modifiers) {
                case ModifierKeys.None:
                    _history.ClearHistoryEntrySelection();
                    _history.SelectHistoryEntry(lineNumber);
                    return false;

                case ModifierKeys.Control:
                    _history.ToggleHistoryEntrySelection(lineNumber);
                    return true;

                //case ModifierKeys.Shift:
                //    return false;

                //case ModifierKeys.Control | ModifierKeys.Shift:
                //    SnapshotPoint? clickPosition = GetBufferPositionFromPoint(point);
                //    if (!clickPosition.HasValue) {
                //        return false;
                //    }

                //    ExtendWordSelection(point);
                //    _doingWordSelection = true;
                //    return true;

                default:
                    return false;
            }
        }

        private bool HandleDoubleClick(InputEventArgs e, ModifierKeys modifiers) {
            switch (modifiers) {
                case ModifierKeys.None:
                    _history.SendSelectedToRepl();
                    return true;

                //case ModifierKeys.Control:
                //case ModifierKeys.Shift:
                //case ModifierKeys.Control | ModifierKeys.Shift:
                //    return HandleSingleClick(e, modifiers);

                default:
                    return true;
            }
        }

        //private bool HandleTripleClick(InputEventArgs e, ModifierKeys modifiers) {
        //    switch (modifiers) {
        //        case ModifierKeys.None:
        //            var lineUnderPoint = GetTextViewLineUnderPoint(GetAdjustedPosition(e, _textView));
        //            if (lineUnderPoint == null) {
        //                return false;
        //            }

        //            SelectLine(lineUnderPoint);
        //            _doingLineSelection = true;
        //            return true;

        //        case ModifierKeys.Control:
        //        case ModifierKeys.Shift:
        //        case ModifierKeys.Control | ModifierKeys.Shift:
        //            return HandleSingleClick(e, modifiers);

        //        default:
        //            return true;
        //    }
        //}


        //private SnapshotPoint? GetBufferPositionFromPoint(Point pt) {
        //    ITextViewLine textLine = GetTextViewLineUnderPoint(pt);

        //    VirtualSnapshotPoint? insertionPoint = textLine?.GetInsertionBufferPositionFromXCoordinate(pt.X);
        //    return insertionPoint?.Position;
        //}

        //internal bool PreprocessMouseMoveByPosition(Point pt, MouseButtonState leftButtonState) {
        //    // If the left button isn't down, we shouldn't handle the mouse move.
        //    if (leftButtonState == MouseButtonState.Released) {
        //        _doingWordSelection = false;
        //        _doingLineSelection = false;
        //    }

        //    // If we're not doing a word selection or line selection then ignore it and let the default handler 
        //    // deal with it.
        //    if (!_doingWordSelection && !_doingLineSelection) {
        //        return false;
        //    }

        //    // _doingLineSelection and _doingWordSelection should not be both true at the same time.
        //    Debug.Assert(!_doingLineSelection || !_doingWordSelection);


        //    return _doingLineSelection 
        //        ? ExtendLineSelection(pt)
        //        : ExtendWordSelection(pt);
        //}

        //private void SelectWordAtBufferPosition(SnapshotPoint clickPosition) {
        //    using (_ignoreSelectionChangedEvents.Increment()) {
        //        VirtualSnapshotPoint point = new VirtualSnapshotPoint(clickPosition);
        //        _editorOperations.SelectAndMoveCaret(point, point);
        //        SelectCurrentWord();
        //    }
        //}

        /// <summary>
        /// Extend the selection to include the clicked position and then grow the selection to 
        /// include the entire word on either side of the selection.
        /// </summary>
        //private bool ExtendWordSelection(Point mousePosition) {
        //    using (_ignoreSelectionChangedEvents.Increment()) {

        //        SnapshotPoint? clickPosition = GetBufferPositionFromPoint(mousePosition);
        //        if (!clickPosition.HasValue) {
        //            return false;
        //        }

        //        // If we don't have an original selection, create one at the anchor point
        //        if (_originalSelectedWord == null) {
        //            SelectWordAtBufferPosition(_textView.Selection.AnchorPoint.Position);
        //        }

        //        if (_originalSelectedWord == null) {
        //            return false;
        //        }

        //        var position = clickPosition.Value.Position;

        //        SnapshotSpan selectionWord = _originalSelectedWord.GetSpan(_textView.TextSnapshot);
        //        var start = Math.Min(selectionWord.Start.Position, position);
        //        var end = Math.Max(selectionWord.End.Position, position);

        //        if (start == selectionWord.Start.Position && end == selectionWord.End.Position) {
        //            return true;
        //        }

        //        // Remember if the selection should be reversed
        //        var isReversed = position == start;
        //        var textStructureNavigator = _textStructureNavigatorProvider.GetTextStructureNavigator(_textView.TextBuffer);

        //        if (position < selectionWord.Start) {
        //            var startExtend = GetTextExtent(textStructureNavigator, start);
        //            if (startExtend.IsSignificant && startExtend.Span.Start < start) {
        //                start = startExtend.Span.Start;
        //            }
        //        }

        //        if (position > selectionWord.End) {
        //            var endExtend = GetTextExtent(textStructureNavigator, end);
        //            if (endExtend.IsSignificant && endExtend.Span.End > end) {
        //                end = endExtend.Span.End;
        //            }
        //        }

        //        _textView.Selection.Mode = TextSelectionMode.Stream;
        //        if (isReversed) {
        //            SelectRange(end, start);
        //        } else {
        //            SelectRange(start, end);
        //        }

        //        return true;
        //    }
        //}

        //private bool ExtendLineSelection(Point mousePosition) {
        //    using (_ignoreSelectionChangedEvents.Increment()) {
        //        if (_originalSelectedLine == null) {
        //            _doingLineSelection = false;
        //            return false;
        //        }

        //        var lineUnderMouse = GetTextViewLineUnderPoint(mousePosition);
        //        if (lineUnderMouse == null) {
        //            return false;
        //        }

        //        SnapshotSpan extentOfLineUnderMouse = lineUnderMouse.ExtentIncludingLineBreak;

        //        var start = Math.Min(extentOfLineUnderMouse.Start, _originalSelectedLine.GetStartPoint(_textView.TextSnapshot));
        //        var end = Math.Max(extentOfLineUnderMouse.End, _originalSelectedLine.GetEndPoint(_textView.TextSnapshot));
        //        var isReversed = lineUnderMouse.Start.Position.Equals(start);

        //        _textView.Selection.Mode = TextSelectionMode.Stream;
        //        if (isReversed) {
        //            SelectRange(end, start);
        //        } else { 
        //            SelectRange(start, end);
        //        }

        //        return true;
        //    }
        //}

        //private void SelectCurrentWord() {
        //    using (_ignoreSelectionChangedEvents.Increment()) {
        //        _textView.Selection.Mode = TextSelectionMode.Stream;
        //        _editorOperations.SelectCurrentWord();
        //        _originalSelectedWord = GetCurrentSelection();
        //    }
        //}

        //private void SelectLine(ITextViewLine line) {
        //    using (_ignoreSelectionChangedEvents.Increment()) {
        //        _textView.Selection.Mode = TextSelectionMode.Stream;
        //        _editorOperations.SelectLine(line, false);
        //        _originalSelectedLine = GetCurrentSelection();
        //    }
        //}

        private int GetLineNumberUnderPoint(Point point) {
            ITextViewLine textLine = GetTextViewLineUnderPoint(point);
            return textLine?.Snapshot.GetLineNumberFromPosition(textLine.Start.Position) ?? -1;
        }

        private ITextViewLine GetTextViewLineUnderPoint(Point pt) {
            return _textView.TextViewLines.GetTextViewLineContainingYCoordinate(pt.Y);
        }

        //private ITrackingSpan GetCurrentSelection() {
        //    return _textView.TextSnapshot.CreateTrackingSpan(_textView.Selection.StreamSelectionSpan.SnapshotSpan, SpanTrackingMode.EdgeExclusive);
        //}

        //private TextExtent GetTextExtent(ITextStructureNavigator textStructureNavigator, int position) {
        //    return textStructureNavigator.GetExtentOfWord(new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, position));
        //}

        //private void SelectRange(int start, int end) {
        //    var startPoint = new SnapshotPoint(_textView.TextSnapshot, start);
        //    var endPoint = new SnapshotPoint(_textView.TextSnapshot, end);
        //    _textView.Selection.Select(new VirtualSnapshotPoint(startPoint), new VirtualSnapshotPoint(endPoint));

        //    ITextViewLine textViewLine = _textView.GetTextViewLineContainingBufferPosition(endPoint);

        //    var affinity = textViewLine.IsLastTextViewLineForSnapshotLine || endPoint != textViewLine.End
        //        ? PositionAffinity.Successor
        //        : PositionAffinity.Predecessor;
        //    _textView.Caret.MoveTo(endPoint, affinity);

        //    var ensureSpanVisibleOptions = start > end 
        //        ? EnsureSpanVisibleOptions.MinimumScroll | EnsureSpanVisibleOptions.ShowStart
        //        : EnsureSpanVisibleOptions.MinimumScroll;
        //    _textView.ViewScroller.EnsureSpanVisible(_textView.Selection.StreamSelectionSpan.SnapshotSpan, ensureSpanVisibleOptions);
        //}
    }
}