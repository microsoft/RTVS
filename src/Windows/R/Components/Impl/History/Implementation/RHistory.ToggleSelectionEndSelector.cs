using System;

namespace Microsoft.R.Components.History.Implementation {
    internal sealed partial class RHistory {
        private sealed class RangeEntrySelector  : IEntrySelector {
            private readonly RHistory _history;
            private readonly bool _isUp;

            public RangeEntrySelector(RHistory history, bool isUp) {
                _history = history;
                _isUp = isUp;
            }

            public void EntriesSelected() {
                var lastSelectedIndex = _history._entries.LastSelectedIndex();
                var index = _isUp ? lastSelectedIndex - 1 : lastSelectedIndex + 1;
                if (index >= 0 && index < _history._entries.Count) {
                    _history._entries.SelectRangeTo(index);
                    _history.OnSelectionChanged();
                }
            }

            public void TextSelected() {
                var selection = _history.VisualComponent.TextView.Selection;
                var anchorPoint = selection.AnchorPoint;
                var snapshot = _history._historyTextBuffer.CurrentSnapshot;
                var caret = _history.VisualComponent.TextView.Caret;
                var activePointLineNumber = selection.ActivePoint.Position.GetContainingLine().LineNumber;
                var newActivePointLineNumber = _isUp
                    ? Math.Max(activePointLineNumber - 1, 0)
                    : Math.Min(activePointLineNumber + 1, snapshot.LineCount - 1);

                if (activePointLineNumber == newActivePointLineNumber) {
                    return;
                }

                caret.MoveTo(_history.VisualComponent.TextView.TextViewLines[newActivePointLineNumber]);
                selection.Select(anchorPoint.TranslateTo(_history._historyTextBuffer.CurrentSnapshot), caret.Position.VirtualBufferPosition);
            }
        }
    }
}