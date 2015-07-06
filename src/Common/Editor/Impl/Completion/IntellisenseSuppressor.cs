using System;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completion
{
    /// <summary>
    /// If user types escape dismissing intellisense window, this class keeps
    /// track if caret position is still in the same text token so completion
    /// controller can avoid re-triggering intellisense that user didn't want.
    /// </summary>
    internal class IntellisenseSuppressor
    {
        private ITextView _textView;

        private IntellisenseSuppressor() { }

        public static IntellisenseSuppressor Create(ITextView textView)
        {
            var intellisenseSuppressor = new IntellisenseSuppressor();

            intellisenseSuppressor.Start(textView);
            if (intellisenseSuppressor.IsActive)
                return intellisenseSuppressor;

            return null;
        }

        public bool IsActive
        {
            get { return _textView != null; }
        }

        private void Start(ITextView textView)
        {
            var position = textView.Caret.Position.BufferPosition.Position;
            var line = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(position);
            if (line != null)
            {
                if (line.Length == 0 || position == line.Start)
                    return;

                var lineText = line.GetText();
                var linePosition = position - line.Start;

                if (linePosition > 0 && linePosition < lineText.Length - 1)
                {
                    if (Char.IsWhiteSpace(lineText[linePosition - 1]) ||
                        Char.IsWhiteSpace(lineText[linePosition]) ||
                        Char.IsWhiteSpace(lineText[linePosition + 1]))
                    {
                        return;
                    }
                }

                _textView = textView;

                _textView.Caret.PositionChanged += OnCaretPositionChanged;
                _textView.TextBuffer.Changed += OnTextBufferChanged;
                TextViewListenerEvents.TextViewDisconnected += OnTextViewDisconnected;
            }
        }

        private void OnTextViewDisconnected(object sender, TextViewListenerEventArgs e)
        {
            if (e.TextView == _textView)
            {
                Stop();
            }
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            foreach (var change in e.Changes)
            {
                if (TextContainsBreakingCharacters(change.OldText) ||
                    TextContainsBreakingCharacters(change.NewText))
                {
                    Stop();
                    break;
                }
            }
        }

        private bool TextContainsBreakingCharacters(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (!Char.IsLetter(ch) && ch != '_' && ch != '-')
                {
                    return true;
                }
            }

            return false;
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            TextViewListenerEvents.TextViewDisconnected -= OnTextViewDisconnected;

            _textView.Caret.PositionChanged -= OnCaretPositionChanged;
            _textView.TextBuffer.Changed -= OnTextBufferChanged;
            _textView = null;
        }
    }
}