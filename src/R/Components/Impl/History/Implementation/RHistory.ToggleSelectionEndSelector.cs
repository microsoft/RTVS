using System;
using Microsoft.VisualStudio.Text;

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
                var lastSelected = _history._entries.LastSelected();
                var rangeEnd = _isUp ? lastSelected.Previous : lastSelected.Next;
                if (rangeEnd != null) {
                    _history._entries.SelectRangeTo(rangeEnd);
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