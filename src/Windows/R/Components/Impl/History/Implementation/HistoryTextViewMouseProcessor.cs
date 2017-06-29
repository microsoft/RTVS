// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.R.Components.History.Implementation {
    internal class HistoryWindowPaneMouseProcessor : MouseProcessorBase, IMouseProcessor2 {
        private readonly IRHistoryVisual _history;

        private TimeSpan _elapsedSinceLastTap;
        private Point _lastTapPosition;
        private Point _currentTapPosition;

        private readonly Stopwatch _doubleTapStopWatch = new Stopwatch();
        private readonly TimeSpan _maximumElapsedDoubleTap = new TimeSpan(0, 0, 0, 0, 600);
        private readonly int _minimumPositionDelta = 30;

        public HistoryWindowPaneMouseProcessor(IRHistoryVisual history) {
            _history = history;
        }

        #region IMouseProcessorProvider Member Implementations

        public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e) {
            HandleLeftButtonDown(e);
        }

        public override void PreprocessMouseRightButtonUp(MouseButtonEventArgs e) {
            _history.VisualComponent.Container.ShowContextMenu(RHistoryCommandIds.ContextMenu, GetPosition(e, _history.VisualComponent.Control));
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Mouse up event
        /// </summary>
        public override void PostprocessMouseUp(MouseButtonEventArgs e) {
            _lastTapPosition = GetAdjustedPosition(e, _history.VisualComponent.TextView);
            _doubleTapStopWatch.Restart();
        }

        public void PreprocessTouchDown(TouchEventArgs e) {
            _currentTapPosition = GetAdjustedPosition(e, _history.VisualComponent.TextView);
            _elapsedSinceLastTap = _doubleTapStopWatch.Elapsed;
            _doubleTapStopWatch.Restart();

            HandleLeftButtonDown(e);

            _lastTapPosition = _currentTapPosition;
        }

        public void PostprocessTouchDown(TouchEventArgs e) { }

        public void PreprocessTouchUp(TouchEventArgs e) { }

        public void PostprocessTouchUp(TouchEventArgs e) { }

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
            if (_history.VisualComponent.TextView.TextBuffer.CurrentSnapshot.Length == 0) {
                return true;
            }

            var point = GetAdjustedPosition(e, _history.VisualComponent.TextView);
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
                    _history.VisualComponent.TextView.Selection.Clear();
                    _history.ToggleHistoryEntrySelection(lineNumber);
                    return true;

                case ModifierKeys.Shift:
                    if (_history.HasSelectedEntries) {
                        _history.SelectHistoryEntriesRangeTo(lineNumber);
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        private bool HandleDoubleClick(InputEventArgs e, ModifierKeys modifiers) {
            switch (modifiers) {
                case ModifierKeys.None:
                    var point = GetAdjustedPosition(e, _history.VisualComponent.TextView);
                    var textLine = GetTextViewLineUnderPoint(point);
                    if (textLine != null) {
                        _history.SendSelectedToRepl();
                    }
                    return true;

                default:
                    return true;
            }
        }

        private int GetLineNumberUnderPoint(Point point) {
            ITextViewLine textLine = GetTextViewLineUnderPoint(point);
            return textLine?.Snapshot.GetLineNumberFromPosition(textLine.Start.Position) ?? -1;
        }

        private ITextViewLine GetTextViewLineUnderPoint(Point pt) {
            return _history.VisualComponent.TextView.TextViewLines.GetTextViewLineContainingYCoordinate(pt.Y);
        }
    }
}