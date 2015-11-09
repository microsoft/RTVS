using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.Languages.Editor.BraceMatch {
    class BraceHighlighter : ITagger<TextMarkerTag>, IDisposable
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private ITextBuffer _textBuffer;
        private ITextView _textView;
        private SnapshotPoint? _currentChar;
        private bool _highlighted = false;
        private IBraceMatcher _braceMatcher;

        public BraceHighlighter(ITextView view, ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _textView = view;

            var importComposer = new ContentTypeImportComposer<IBraceMatcherProvider>(EditorShell.Current.CompositionService);
            var braceMatcherProvider = importComposer.GetImport(textBuffer.ContentType.TypeName);
            if (braceMatcherProvider != null)
            {
                _braceMatcher = braceMatcherProvider.CreateBraceMatcher(view, textBuffer);

                view.LayoutChanged += OnViewLayoutChanged;
                view.Caret.PositionChanged += OnCaretPositionChanged;

                ServiceManager.AddService<BraceHighlighter>(this, _textView);
            }
        }

        void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            if (CanHighlight(_textView) || _highlighted)
            {
                IdleTimeAction.Create(() => UpdateAtCaretPosition(), 150, this);
            }
        }

        void OnViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot) // make sure that there has really been a change
            {
                if (CanHighlight(_textView) || _highlighted)
                {
                    IdleTimeAction.Create(() => UpdateAtCaretPosition(), 150, this);
                }
            }
        }

        void UpdateAtCaretPosition()
        {
            // Check for disposal, this can be disposed while waiting for idle
            if (_textView != null && _braceMatcher != null)
            {
                var caretPosition = _textView.Caret.Position;
                _currentChar = caretPosition.Point.GetPoint(_textBuffer, caretPosition.Affinity);

                // We need to clear current highlight if caret went to another buffer

                _highlighted = false;
                if (TagsChanged != null)
                    TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, 0,
                        _textBuffer.CurrentSnapshot.Length)));
            }
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _braceMatcher == null || _textBuffer == null)
                yield break;

            if (!_currentChar.HasValue || _currentChar.Value.Position > _currentChar.Value.Snapshot.Length)
                yield break;

            SnapshotPoint position = _currentChar.Value;
            ITextSnapshot snapshot = position.Snapshot;

            if (spans[0].Snapshot.TextBuffer != snapshot.TextBuffer)
            {
                // This happens with diff views. The position could be mapped, but is it worth it?
                yield break;
            }

            if (spans[0].Snapshot != snapshot)
                position = position.TranslateTo(spans[0].Snapshot, PointTrackingMode.Positive);

            int start, end;
            if (!_braceMatcher.GetBracesFromPosition(snapshot, position, false, out start, out end))
                yield break;

            yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(snapshot, start, 1), new BraceHighlightTag());
            yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(snapshot, end, 1), new BraceHighlightTag());

            _highlighted = true;
        }

        private static bool CanHighlight(ITextView textView)
        {
            // The view can be null when this function is called during an event chain
            // that disposes this object. Detect disposal:
            if (textView != null)
            {
                var caretPosition = textView.Caret.Position.BufferPosition;
                var snapshot = textView.TextBuffer.CurrentSnapshot;

                return IsHighlightablePosition(snapshot, caretPosition);
            }

            return false;
        }

        private static bool IsHighlightablePosition(ITextSnapshot snapshot, int caretPosition)
        {
            bool highlitable = false;

            if (caretPosition < snapshot.Length)
                highlitable = IsHighlightableCharacter(snapshot[caretPosition]);

            if (!highlitable && caretPosition > 0)
                highlitable = IsHighlightableCharacter(snapshot[caretPosition - 1]);

            return highlitable;
        }

        private static bool IsHighlightableCharacter(char ch)
        {
            switch (ch)
            {
                case '{':
                case '}':
                case '[':
                case ']':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        void IDisposable.Dispose()
        {
            if (_textView != null)
            {
                _textView.LayoutChanged -= OnViewLayoutChanged;
                _textView.Caret.PositionChanged -= OnCaretPositionChanged;

                ServiceManager.RemoveService<BraceHighlighter>(_textView);

                _textView = null;
            }

            _textBuffer = null;
        }
    }
}
