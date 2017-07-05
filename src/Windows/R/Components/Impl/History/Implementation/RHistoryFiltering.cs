// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.R.Components.History.Implementation {
    internal class RHistoryFiltering : IRHistoryFiltering {
        private const string IntraTextAdornmentBufferKey = "IntraTextAdornmentBuffer";

        private readonly IRHistoryVisual _history;
        private readonly IRSettings _settings;
        private readonly ITextSearchService2 _textSearchService;
        private readonly ITextView _textView;
        private readonly ITextBuffer _textBuffer;
        private readonly IElisionBuffer _elisionBuffer;

        private string _searchPattern;

        public RHistoryFiltering(IRHistoryVisual history, IRHistoryWindowVisualComponent visualComponent, IRSettings settings, ITextSearchService2 textSearchService) {
            _history = history;
            _history.HistoryChanging += HistoryChanging;
            _history.HistoryChanged += HistoryChanged;

            _settings = settings;
            _textSearchService = textSearchService;
            _textView = visualComponent.TextView;
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

        private void HistoryChanging(object sender, EventArgs args) {
            if (_searchPattern == null) {
                return;
            }

            var span = new Span(0, _textBuffer.CurrentSnapshot.Length);
            _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(span));
        }

        private void HistoryChanged(object sender, EventArgs args) {
            if (_searchPattern == null) {
                return;
            }

            if (_settings.ClearFilterOnAddHistory) {
                ClearFilter();
            } else if (_history.HasEntries) {
                FilterImpl(_searchPattern);
            }
        }

        private void FilterImpl(string searchPattern) {
            var snapshot = _textBuffer.CurrentSnapshot;
            var entrySpans = _history.GetAllHistoryEntrySpans();
            Debug.Assert(entrySpans.Any());

            var spansToShow = new List<Span>();
            var spansToHide = new List<Span>();

            var span = entrySpans[0];
            var showSpan = _textSearchService.Find(span, span.Start, searchPattern, FindOptions.Multiline).HasValue;

            for (var i = 1; i < entrySpans.Count; i++) {
                var nextSpan = entrySpans[i];
                var showNextSpan = _textSearchService.Find(nextSpan, nextSpan.Start, searchPattern, FindOptions.Multiline).HasValue;
                if (showNextSpan) {
                    if (showSpan) {
                        span = new SnapshotSpan(span.Start, nextSpan.End);
                    } else if (spansToShow.Any()) {
                        spansToHide.Add(span);
                        span = new SnapshotSpan(span.End, nextSpan.End);
                    } else {
                        spansToHide.Add(new SnapshotSpan(span.Start, nextSpan.Start));
                        span = nextSpan;
                    }
                    showSpan = true;
                } else {
                    if (!showSpan) {
                        span = new SnapshotSpan(span.Start, nextSpan.End);
                    } else {
                        spansToShow.Add(span);
                        span = new SnapshotSpan(span.End, nextSpan.End);
                    }
                    showSpan = false;
                }
            }

            // Add last span
            if (showSpan) {
                spansToShow.Add(span);
            } else {
                spansToHide.Add(span);
            }

            if (spansToShow.Count == 0) {
                if (_elisionBuffer.CurrentSnapshot.Length == 0) {
                    return;
                }

                _elisionBuffer.ModifySpans(new NormalizedSpanCollection(new Span(0, snapshot.Length)), new NormalizedSpanCollection(new Span(0, 0)));
                return;
            }

            _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(new Span(0, snapshot.Length)));

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
        }
    }
}