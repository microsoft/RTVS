using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.History {
    internal sealed class RHistory : IRHistory {
        private const string BlockSeparator = "\r\n";
        private const string LineSeparator = "\f";

        private readonly List<RHistoryBlock> _entries = new List<RHistoryBlock>();
        private readonly ITextView _textView;
        private readonly IFileSystem _fileSystem;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly ITextBuffer _historyTextBuffer;
        private readonly CountdownDisposable _textBufferIsEditable;

        private IReadOnlyRegion _region;

        public event EventHandler<EventArgs> SelectionChanged;

        public RHistory(ITextView textView, IFileSystem fileSystem, IEditorOperationsFactoryService editorOperationsFactory) {
            _textView = textView;
            _historyTextBuffer = textView.TextBuffer;
            _fileSystem = fileSystem;
            _editorOperationsFactory = editorOperationsFactory;

            _textBufferIsEditable = new CountdownDisposable(MakeTextBufferReadOnly);
            MakeTextBufferReadOnly();
        }

        public bool TryLoadFromFile(string path) {
            string[] historyLines;
            try {
                historyLines = _fileSystem.FileReadAllLines(path);
            } catch (Exception) {
                // .RHistory file isn't mandatory for r session, so if it can't be loaded, just exit
                return false;
            }

            ClearTrackingSpans();

            _entries.Clear();
            foreach (var historyLine in historyLines) {
                _entries.Add(new RHistoryBlock(historyLine.Replace(LineSeparator, BlockSeparator)));
            }

            SetTrackingSpans();

            // interactiveWindow.AddInput doesn't work correctly in VS RTM, need to check it in Update 1

            //var interactiveWindow = ReplWindow.Current.GetInteractiveWindow().InteractiveWindow;
            //interactiveWindow.Operations.ClearHistory();
            //foreach (var historyBlock in _entries) {
            //    interactiveWindow.AddInput(historyBlock.Text);
            //}
            return true;
        }

        public bool TrySaveToFile(string path) {
            var content = _entries.Select(e => e.Text.Replace(BlockSeparator, LineSeparator)).ToArray();
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

            var editorOperations = _editorOperationsFactory.GetEditorOperations(textView);
            var selectedText = GetSelectedText();
            if (textView.Selection.IsEmpty) {
                editorOperations.InsertText(selectedText);
            } else if (textView.Selection.Mode == TextSelectionMode.Box) {
                VirtualSnapshotPoint _, __;
                editorOperations.InsertTextAsBox(selectedText, out _, out __);
            } else {
                editorOperations.ReplaceSelection(selectedText);
            }
        }

        public IList<SnapshotSpan> GetSelectedHistoryEntrySpans() {
            var snapshotSpans = new List<SnapshotSpan>();
            if (!_entries.Any()) {
                return snapshotSpans;
            }

            var snapshot = _historyTextBuffer.CurrentSnapshot;

            SnapshotPoint start = new SnapshotPoint(snapshot, 0);
            for (int i = 0; i < _entries.Count; i++) {
                var historyBlock = _entries[i];
                if (!historyBlock.IsSelected) {
                    continue;
                }

                if (i == 0 || !_entries[i-1].IsSelected) {
                    start = historyBlock.TrackingSpan.GetStartPoint(snapshot);
                }

                if (i == _entries.Count - 1 || !_entries[i+1].IsSelected) {
                    var end = historyBlock.TrackingSpan.GetEndPoint(snapshot);
                    snapshotSpans.Add(new SnapshotSpan(start, end));
                }
            }

            return snapshotSpans;
        }

        public string GetSelectedText() {
            var selectedText = string.Join(BlockSeparator, 
                _entries.Where(b => b.IsSelected).Select(b => b.Text));

            if (selectedText.Length > 0 || _textView.Selection.IsEmpty) {
                return selectedText;
            }

            return string.Join(BlockSeparator, _textView.Selection.SelectedSpans.Select(s => s.GetText()));
        }

        public SnapshotSpan SelectHistoryEntry(int lineNumber) {
            var historyBlock = GetHistoryBlockFromLineNumber(lineNumber);
            if (!historyBlock.IsSelected) {
                historyBlock.IsSelected = true;
                SelectionChanged?.Invoke(this, new EventArgs());
            }

            return historyBlock.TrackingSpan.GetSpan(_historyTextBuffer.CurrentSnapshot);
        }

        public SnapshotSpan DeselectHistoryEntry(int lineNumber) {
            var historyBlock = GetHistoryBlockFromLineNumber(lineNumber);
            if (!historyBlock.IsSelected) {
                historyBlock.IsSelected = false;
                SelectionChanged?.Invoke(this, new EventArgs());
            }

            return historyBlock.TrackingSpan.GetSpan(_historyTextBuffer.CurrentSnapshot);
        }

        public SnapshotSpan ToggleHistoryEntrySelection(int lineNumber) {
            var historyBlock = GetHistoryBlockFromLineNumber(lineNumber);
            historyBlock.IsSelected = !historyBlock.IsSelected;
            SelectionChanged?.Invoke(this, new EventArgs());
            return historyBlock.TrackingSpan.GetSpan(_historyTextBuffer.CurrentSnapshot);
        }

        public void ClearHistoryEntrySelection() {
            bool raiseEvent = false;
            foreach (var historyBlock in _entries) {
                raiseEvent |= historyBlock.IsSelected;
                historyBlock.IsSelected = false;
            }

            if (raiseEvent) {
                SelectionChanged?.Invoke(this, new EventArgs());
            }
        }

        public void DeleteSelectedHistoryEntries() {
            throw new NotImplementedException();
        }

        public void AddToHistory(string text) {
            text = text.TrimEnd('\r', '\n');
            if (string.IsNullOrWhiteSpace(text)) {
                return;
            }

            var historyBlock = new RHistoryBlock(text);
            var snapshot = _historyTextBuffer.CurrentSnapshot;

            using (EditTextBuffer()) {
                if (_entries.Any()) {
                    snapshot = _historyTextBuffer.Insert(snapshot.Length, BlockSeparator);
                }

                var position = snapshot.Length;
                snapshot = _historyTextBuffer.Insert(position, text);

                historyBlock.TrackingSpan = snapshot.CreateTrackingSpan(new Span(position, text.Length), SpanTrackingMode.EdgeExclusive);
            }

            _entries.Add(historyBlock);
        }

        private string GetWholeHistory() => string.Join(BlockSeparator, _entries.Select(s => s.Text));

        private void SetTrackingSpans() {
            if (_historyTextBuffer == null) {
                return;
            }

            using (EditTextBuffer()) {
                var snapshot = _historyTextBuffer.Replace(new Span(0, _historyTextBuffer.CurrentSnapshot.Length), GetWholeHistory());

                var position = 0;
                foreach (var historyBlock in _entries) {
                    var span = new Span(position, historyBlock.Text.Length);
                    historyBlock.TrackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
                    position = snapshot.GetLineFromPosition(position).EndIncludingLineBreak;
                }
            }
        }

        private void ClearTrackingSpans() {
            using (EditTextBuffer()) {
                _historyTextBuffer?.Delete(new Span(0, _historyTextBuffer.CurrentSnapshot.Length));
                foreach (var historyBlock in _entries) {
                    historyBlock.TrackingSpan = null;
                }
            }
        }

        private IDisposable EditTextBuffer() {
            if (_region != null && _historyTextBuffer != null) {
                using (var edit = _historyTextBuffer.CreateReadOnlyRegionEdit()) {
                    edit.RemoveReadOnlyRegion(_region);
                    _region = null;
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
                _region = edit.CreateReadOnlyRegion(span, SpanTrackingMode.EdgeInclusive, EdgeInsertionMode.Deny);
                edit.Apply();
            }
        }

        private RHistoryBlock GetHistoryBlockFromLineNumber(int lineNumber) {
            var snapshot = _historyTextBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            return _entries.First(b => b.TrackingSpan != null && b.TrackingSpan.GetSpan(snapshot).Contains(line.Extent));
        }
    }
}