using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.History {
    internal class RHistoryFiltering : IRHistoryFiltering {
        private const string IntraTextAdornmentBufferKey = "IntraTextAdornmentBuffer";

        private readonly IRHistory _history;
        private readonly IRToolsSettings _settings;
        private readonly ITextSearchService2 _textSearchService;
        private readonly ITextView _textView;
        private readonly ITextBuffer _textBuffer;
        private readonly IElisionBuffer _elisionBuffer;

        private string _searchPattern;

        public RHistoryFiltering(IRHistory history, IRToolsSettings settings, ITextSearchService2 textSearchService) {
            _history = history;
            _history.HistoryChanged += HistoryChanged;

            _settings = settings;
            _textSearchService = textSearchService;
            _textView = _history.GetOrCreateTextView();
            _textBuffer = _textView.TextDataModel.DataBuffer;

            IElisionBuffer elisionBuffer;
            _textView.TextViewModel.Properties.TryGetProperty(IntraTextAdornmentBufferKey, out elisionBuffer);
            _elisionBuffer = elisionBuffer;
        }

        public void Filter(string searchPattern) {
            if (!_history.HasEntries || _searchPattern.EqualsIgnoreCase(searchPattern)) {
                return;
            }

            if (string.IsNullOrEmpty(searchPattern)) {
                ClearFilter();
                return;
            }

            _searchPattern = searchPattern;

            FilterImpl(searchPattern);
        }

        private void HistoryChanged(object sender, EventArgs args) {
            if (_searchPattern == null) {
                return;
            }

            if (_settings.ClearFilterOnAddHistory) {
                ClearFilter();
            } else {
                FilterImpl(_searchPattern);
            }
        }

        private void FilterImpl(string searchPattern) {
            var snapshot = _textBuffer.CurrentSnapshot;
            var entries = _history.GetAllHistoryEntrySpans();
            var startPoints = entries.Select(e => e.Start).ToList();
            var endPoints = startPoints.Skip(1).Append(entries[entries.Count - 1].End);
            var spans = startPoints.Zip(endPoints, (start, end) => new SnapshotSpan(start, end));

            _history.ClearHistoryEntrySelection();
            _textView.Selection.Clear();

            IList<Span> spansToShow;
            IList<Span> spansToHide;
            spans.Split(s => _textSearchService.Find(s, s.Start, searchPattern, FindOptions.Multiline).HasValue, s => new Span(s.Start, s.Length), out spansToShow,
                out spansToHide);

            if (spansToShow.Count == 0) {
                if (_elisionBuffer.CurrentSnapshot.Length == 0) {
                    return;
                }

                _history.Workaround169159(_elisionBuffer);
                //Uncomment lines when bug #169159 is fixed: https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems/edit/169159
                //_textView.Caret.MoveTo(new SnapshotPoint(snapshot, 0));
                //_elisionBuffer.ElideSpans(new NormalizedSpanCollection(new Span(0, snapshot.Length)));
                return;
            }

            _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(new Span(0, _textBuffer.CurrentSnapshot.Length)));

            MoveCaretToVisiblePoint(spansToShow, snapshot);

            if (spansToHide.Count != 0) {
                _elisionBuffer.ElideSpans(new NormalizedSpanCollection(spansToHide));
            }

            _textView.Caret.EnsureVisible();
        }

        private void MoveCaretToVisiblePoint(IList<Span> spansToShow, ITextSnapshot snapshot) {
            var caretPosition = _textView.Caret.Position.BufferPosition.Position;

            Span? previousSpan = null;
            foreach (var span in spansToShow) {
                if (span.Contains(caretPosition)) {
                    return;
                }

                if (span.Start > caretPosition) {
                    var newCaretPosition = previousSpan?.End ?? span.Start;
                    _textView.Caret.MoveTo(new SnapshotPoint(snapshot, newCaretPosition));
                    return;
                }

                previousSpan = span;
            }
        }

        public void ClearFilter() {
            if (!_history.HasEntries) {
                return;
            }

            _history.ClearHistoryEntrySelection();
            _textView.Selection.Clear();

            _searchPattern = null;
            var span = new Span(0, _textBuffer.CurrentSnapshot.Length);
            _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(span));
            _textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(_textView.TextSnapshot, new Span(0, 0)));
        }
    }
}