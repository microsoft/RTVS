using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.History {
    internal sealed class RHistory : IRHistory {
        private const string BlockSeparator = "\r\n";
        private const string LineSeparator = "\u00a0";

        private readonly RHistoryEntries _entries = new RHistoryEntries();
        private readonly ITextView _textView;
        private readonly IFileSystem _fileSystem;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IElisionBuffer _elisionBuffer;
        private readonly ITextBuffer _historyTextBuffer;
        private readonly CountdownDisposable _textBufferIsEditable;
        private readonly IEditorOperations _editorOperations;
        private readonly IRtfBuilderService _rtfBuilderService;
        private readonly ITextSearchService2 _textSearchService;
        private readonly IVsUIShell _vsUiShell;

        private IReadOnlyRegion _readOnlyRegion;

        public event EventHandler<EventArgs> HistoryChanged;
        public event EventHandler<EventArgs> SelectionChanged;

        public RHistory(ITextView textView, IFileSystem fileSystem, IEditorOperationsFactoryService editorOperationsFactory, IElisionBuffer elisionBuffer, IRtfBuilderService rtfBuilderService, ITextSearchService2 textSearchService, IVsUIShell vsShell) {
            _textView = textView;
            _historyTextBuffer = textView.TextDataModel.DataBuffer;
            _fileSystem = fileSystem;
            _editorOperationsFactory = editorOperationsFactory;
            _elisionBuffer = elisionBuffer;
            _rtfBuilderService = rtfBuilderService;
            _vsUiShell = vsShell;
            _editorOperations = _editorOperationsFactory.GetEditorOperations(_textView);
            _textSearchService = textSearchService;

            _textBufferIsEditable = new CountdownDisposable(MakeTextBufferReadOnly);
            MakeTextBufferReadOnly();
        }

        public bool HasEntries => _entries.HasEntries;
        public bool HasSelectedEntries => _entries.HasSelectedEntries;

        public bool TryLoadFromFile(string path) {
            string[] historyLines;
            try {
                historyLines = _fileSystem.FileReadAllLines(path).ToArray();
            } catch (Exception) {
                // .RHistory file isn't mandatory for r session, so if it can't be loaded, just exit
                return false;
            }

            DeleteTrackingSpans();

            _entries.RemoveAll();

            foreach (var historyLine in historyLines.Where(l => !string.IsNullOrWhiteSpace(l))) {
                _entries.Add(historyLine.Replace(LineSeparator, BlockSeparator));
            }

            SetTrackingSpans();

            return true;
        }

        public bool TrySaveToFile(string path) {
            var content = _entries.GetEntriesText().Select(t => t.Replace(BlockSeparator, LineSeparator)).ToArray();
            try {
                _fileSystem.FileWriteAllLines(path, content);
                return true;
            } catch (Exception) {
                // Failure to save .RHistory isn't critical
                return false;
            }
        }

        public void SendSelectedToRepl() {
            var selectedText = GetSelectedText();
            ReplWindow.Current.ReplaceCurrentExpression(selectedText);
        }

        public void SendSelectedToTextView(IWpfTextView textView) {
            var targetTextViewEditorOperations = _editorOperationsFactory.GetEditorOperations(textView);
            var selectedText = GetSelectedText();
            if (textView.Selection.IsEmpty) {
                targetTextViewEditorOperations.InsertText(selectedText);
            } else if (textView.Selection.Mode == TextSelectionMode.Box) {
                VirtualSnapshotPoint _, __;
                targetTextViewEditorOperations.InsertTextAsBox(selectedText, out _, out __);
            } else {
                targetTextViewEditorOperations.ReplaceSelection(selectedText);
            }
        }

        public void CopySelection() {
            var selectedEntries = GetSelectedHistoryEntrySpans();
            if (!selectedEntries.Any()) {
                _editorOperations.CopySelection();
            }
            
            var normalizedCollection = new NormalizedSnapshotSpanCollection(selectedEntries);
            var text = GetSelectedText();
            var rtf = _rtfBuilderService.GenerateRtf(normalizedCollection, _textView);
            var data = new DataObject();
            data.SetText(text, TextDataFormat.Text);
            data.SetText(text, TextDataFormat.UnicodeText);
            data.SetText(rtf, TextDataFormat.Rtf);
            data.SetData(DataFormats.StringFormat, text);
            Clipboard.SetDataObject(data, false);
        }

        public IReadOnlyList<SnapshotSpan> GetSelectedHistoryEntrySpans() {
            var snapshotSpans = new List<SnapshotSpan>();
            if (!HasSelectedEntries) {
                return snapshotSpans;
            }

            var snapshot = _historyTextBuffer.CurrentSnapshot;
            var start = new SnapshotPoint(snapshot, 0);
            foreach (var entry in _entries.GetSelectedEntries()) {
                if (entry.Previous == null || !entry.Previous.IsSelected) {
                    start = entry.TrackingSpan.GetStartPoint(snapshot);
                }

                if (entry.Next == null || !entry.Next.IsSelected) {
                    var end = entry.TrackingSpan.GetEndPoint(snapshot);
                    snapshotSpans.Add(new SnapshotSpan(start, end));
                }
            }

            return snapshotSpans;
        }

        public string GetSelectedText() {
            var selectedText = string.Join(BlockSeparator, _entries.GetSelectedEntriesText());

            if (selectedText.Length > 0 || _textView.Selection.IsEmpty) {
                return selectedText;
            }

            return string.Join(BlockSeparator, _textView.Selection.SelectedSpans.Select(s => s.GetText()));
        }

        public SnapshotSpan SelectHistoryEntry(int lineNumber) {
            var entry = GetHistoryBlockFromLineNumber(lineNumber);
            if (!entry.IsSelected) {
                entry.IsSelected = true;
                OnSelectionChanged();
            }

            return entry.TrackingSpan.GetSpan(_historyTextBuffer.CurrentSnapshot);
        }

        public SnapshotSpan DeselectHistoryEntry(int lineNumber) {
            var entry = GetHistoryBlockFromLineNumber(lineNumber);
            if (!entry.IsSelected) {
                entry.IsSelected = false;
                OnSelectionChanged();
            }

            return entry.TrackingSpan.GetSpan(_historyTextBuffer.CurrentSnapshot);
        }

        public SnapshotSpan ToggleHistoryEntrySelection(int lineNumber) {
            var entry = GetHistoryBlockFromLineNumber(lineNumber);
            entry.IsSelected = !entry.IsSelected;
            OnSelectionChanged();
            return entry.TrackingSpan.GetSpan(_historyTextBuffer.CurrentSnapshot);
        }

        public void SelectAllEntries() {
            if (!HasEntries) {
                return;
            }

            _entries.SelectAll();
            OnSelectionChanged();
        }

        public void ClearHistoryEntrySelection() {
            if (!HasSelectedEntries) {
                return;
            }

            _entries.UnselectAll();
            OnSelectionChanged();
        }

        public void DeleteSelectedHistoryEntries() {
            if (!HasSelectedEntries) {
                return;
            }

            DeleteSelectedTrackingSpans();
            _entries.RemoveSelected();
            OnSelectionChanged();
        }

        public void DeleteAllHistoryEntries() {
            var raiseEvent = _entries.HasSelectedEntries;

            DeleteTrackingSpans();
            _entries.RemoveAll();

            if (raiseEvent) {
                OnSelectionChanged();
            }
        }

        public void Filter(string searchPattern) {
            if (!_entries.HasEntries) {
                return;
            }

            if (string.IsNullOrEmpty(searchPattern)) {
                ClearFilter();
                return;
            }

            var snapshot = _historyTextBuffer.CurrentSnapshot;
            var entries = _entries.GetEntries();
            var startPoints = entries.Select(e => e.TrackingSpan.GetStartPoint(snapshot)).ToList();
            var endPoints = startPoints.Skip(1).Append(entries[entries.Count - 1].TrackingSpan.GetEndPoint(snapshot));
            var spans = startPoints.Zip(endPoints, (start, end) => new SnapshotSpan(start, end));

            IList<Span> spansToShow;
            IList<Span> spansToHide;
            spans.Split(s => _textSearchService.Find(s, s.Start, searchPattern, FindOptions.Multiline).HasValue, s => new Span(s.Start, s.Length), out spansToShow, out spansToHide);

            if (spansToShow.Count == 0) {
                if (_elisionBuffer.CurrentSnapshot.Length == 0) {
                    return;
                }

                Workaround169159();
                //Uncomment lines when bug #169159 is fixed: https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems/edit/169159
                //_textView.Caret.MoveTo(new SnapshotPoint(snapshot, 0));
                //_elisionBuffer.ElideSpans(new NormalizedSpanCollection(new Span(0, snapshot.Length)));
                return;
            }

            MoveCaretToVisiblePoint(spansToShow, snapshot);

            if (spansToHide.Count == 0) {
                _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(new Span(0, snapshot.Length)));
            } else {
                _elisionBuffer.ModifySpans(new NormalizedSpanCollection(spansToHide), new NormalizedSpanCollection(spansToShow));
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

        //Remove this method when bug #169159 is fixed: https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems/edit/169159
        private void Workaround169159() {
            using (EditTextBuffer()) {
                _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(new Span(0, _historyTextBuffer.CurrentSnapshot.Length)));
                _historyTextBuffer.Insert(0, "\u200B");
                _textView.Caret.MoveTo(new SnapshotPoint(_historyTextBuffer.CurrentSnapshot, 0));
                _elisionBuffer.ElideSpans(new NormalizedSpanCollection(new Span(1, _historyTextBuffer.CurrentSnapshot.Length - 1)));
                _historyTextBuffer.Delete(new Span(0, 1));
            }
        }

        public void ClearFilter() {
            if (!_entries.HasEntries) {
                return;
            }

            var span = new Span(0, _historyTextBuffer.CurrentSnapshot.Length);
            _elisionBuffer.ExpandSpans(new NormalizedSpanCollection(span));
            _textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(_textView.TextSnapshot, new Span(0, 0)));
        }

        public void AddToHistory(string text) {
            if (RToolsSettings.Current)
            ClearFilter();

            text = text.TrimEnd('\r', '\n');
            if (string.IsNullOrWhiteSpace(text)) {
                return;
            }

            var hasEntries = _entries.HasEntries;
            var entry = _entries.Add(text);
            var snapshot = _historyTextBuffer.CurrentSnapshot;

            using (EditTextBuffer()) {
                if (hasEntries) {
                    snapshot = _historyTextBuffer.Insert(snapshot.Length, BlockSeparator);
                }

                var position = snapshot.Length;
                snapshot = _historyTextBuffer.Insert(position, text);

                entry.TrackingSpan = snapshot.CreateTrackingSpan(new Span(position, text.Length), SpanTrackingMode.EdgeExclusive);
            }
        }

        private string GetWholeHistory() => string.Join(BlockSeparator, _entries.GetEntriesText());

        private void SetTrackingSpans() {
            if (_historyTextBuffer == null) {
                return;
            }

            using (EditTextBuffer()) {
                var snapshot = _historyTextBuffer.Replace(new Span(0, _historyTextBuffer.CurrentSnapshot.Length), GetWholeHistory());

                var position = 0;
                foreach (var entry in _entries.GetEntries()) {
                    var span = new Span(position, entry.Text.Length);
                    entry.TrackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
                    position = snapshot.GetLineFromPosition(span.End).EndIncludingLineBreak;
                }
            }
        }

        private void DeleteTrackingSpans() {
            using (EditTextBuffer()) {
                _historyTextBuffer?.Delete(new Span(0, _historyTextBuffer.CurrentSnapshot.Length));
                foreach (var entry in _entries.GetEntries()) {
                    entry.TrackingSpan = null;
                }
            }
        }

        private void DeleteSelectedTrackingSpans() {
            var selectedEntries = _entries.GetSelectedEntries();
            using (EditTextBuffer()) {
                foreach (var entry in selectedEntries) {
                    var snapshot = _historyTextBuffer.CurrentSnapshot;
                    var startPoint = entry.Previous?.TrackingSpan.GetEndPoint(snapshot) ?? entry.TrackingSpan.GetStartPoint(snapshot);
                    var endPoint = entry.TrackingSpan.GetEndPoint(snapshot);
                    var span = new SnapshotSpan(startPoint, endPoint);
                    _historyTextBuffer.Delete(span);
                }
            }
        }

        private IDisposable EditTextBuffer() {
            if (_readOnlyRegion != null && _historyTextBuffer != null) {
                using (var edit = _historyTextBuffer.CreateReadOnlyRegionEdit()) {
                    edit.RemoveReadOnlyRegion(_readOnlyRegion);
                    _readOnlyRegion = null;
                    edit.Apply();
                }
            }

            return _textBufferIsEditable.Increment();
        }

        private void MakeTextBufferReadOnly() {
            if (_historyTextBuffer == null) {
                return;
            }

            using (var edit = _historyTextBuffer.CreateReadOnlyRegionEdit()) {
                var span = new Span(0, edit.Snapshot.Length);
                _readOnlyRegion = edit.CreateReadOnlyRegion(span, SpanTrackingMode.EdgeInclusive, EdgeInsertionMode.Deny);
                edit.Apply();
            }

            HistoryChanged?.Invoke(this, new EventArgs());
        }

        private void OnSelectionChanged() {
            SelectionChanged?.Invoke(this, new EventArgs());
            _vsUiShell.UpdateCommandUI(0);
        }

        private IRHistoryEntry GetHistoryBlockFromLineNumber(int lineNumber) {
            var snapshot = _historyTextBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            return _entries.Find(b => b.TrackingSpan != null && b.TrackingSpan.GetSpan(snapshot).Contains(line.Extent));
        }
    }
}