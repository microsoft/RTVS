using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Completion.TypeThrough
{
    public class TypeThroughController : IIntellisenseController
    {
        private ITextBuffer _textBuffer;
        private ITextView _textView;

        private List<ProvisionalText> _provisionalTexts;
        private char _typedChar;
        private bool _processing;
        private int _caretPosition;
        private int _bufferVersionWaterline;
        private bool _connected;

        public TypeThroughController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            _textBuffer = subjectBuffers[0];
            _textView = textView;

            _textBuffer.Changed += TextBuffer_Changed;
            _textBuffer.PostChanged += TextBuffer_PostChanged;

            _provisionalTexts = new List<ProvisionalText>();
            _bufferVersionWaterline = _textBuffer.CurrentSnapshot.Version.ReiteratedVersionNumber;

            _connected = true;

            // This object isn't released on content type changes, instead using the (Dis)ConnectSubjectBuffer
            //   methods to control it's lifetime.
            ServiceManager.AddService<TypeThroughController>(this, textView);
        }

        // Test usage only
        internal IEnumerable<ProvisionalText> ProvisionalTexts
        {
            get { return _provisionalTexts;  }
        }

        /// <summary>
        /// Check if this is a controller for the currently active view. This prevents issues 
        /// in split view, or if the same text buffer is opened more than ones.
        /// </summary>
        /// <returns>True is this is the currently active type-through controller, false otherwise.</returns>
        private bool IsActiveController
        {
            get
            {
                return _textView.HasAggregateFocus;
            }
        }

        public static TypeThroughController FromView(ITextView textView)
        {
            return ServiceManager.GetService<TypeThroughController>(textView);
        }

        /// <summary>
        /// Can a typethrough character get inserted after the given position and character?
        /// </summary>
        protected virtual bool CanCompleteAfter(ITextBuffer textBuffer, int prevPosition, char typedCharacter)
        {
            return true;
        }

        /// <summary>
        /// Can a typethrough character get inserted before the given position and character?
        /// </summary>
        protected virtual bool CanCompleteBefore(ITextBuffer textBuffer, int nextPosition, char typedCharacter)
        {
            return StaticCanCompleteBefore(
                _textView,
                textBuffer,
                nextPosition,
                typedCharacter,
                c => IsAllowedLeadingStringChar(c),
                GetInnerProvisionalText());
        }

        protected virtual bool IsAllowedLeadingStringChar(char c)
        {
            return !char.IsWhiteSpace(c);
        }

        /// <summary>
        /// Helper function so that HTML can share logic with the other languages
        /// </summary>
        public static bool StaticCanCompleteBefore(
            ITextView textView,
            ITextBuffer textBuffer,
            int nextPosition,
            char typedCharacter,
            Func<char, bool> isAllowedLeadingStringChar,
            ProvisionalText innerProvisional)
        {
            if (innerProvisional != null && innerProvisional.TrackingSpan != null)
            {
                int? endPos = innerProvisional.CurrentSpan.End - 1;

                if (endPos.HasValue && endPos.Value == nextPosition)
                {
                    // Always allow new provisional text right before the end of existing provisional text
                    return true;
                }
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            char nextCharacter = snapshot[nextPosition];

            if ((typedCharacter == '\'') || (typedCharacter == '\"'))
            {
                if (isAllowedLeadingStringChar(nextCharacter))
                {
                    return false;
                }

                // For a quote, don't add an extra quote if they aren't "unbalanced" on the current line.
                ITextSnapshotLine currentLine = snapshot.GetLineFromPosition(nextPosition);
                string lineText = currentLine.GetText();

                int matchingCharacterCount = lineText.Count(c => c == typedCharacter);
                if (matchingCharacterCount % 2 == 0)
                {
                    return false;
                }
            }
            else if (typedCharacter == '{')
            {
                // For "{", always enable typethrough
                return true;
            }
            else if (char.IsLetterOrDigit(nextCharacter) || nextCharacter == typedCharacter)
            {
                // Dev12 bug 735062 - Don't enable typethrough before certain characters
                return false;
            }

            return true;
        }

        void TextBuffer_PostChanged(object sender, System.EventArgs e)
        {
            if (IsActiveController && !_processing && _typedChar != '\0')
            {
                // if buffer is projected, avoid completion in case when character is at the last position 
                // of the projected span and is equal tothe nearest character to the right in the view 
                // buffer. For example, completion of " in style="foo|" where completion of " in CSS will 
                // interfere with quotes in top HTML buffer.

                if (_textBuffer != _textView.TextBuffer)
                {
                    var viewCaretPosition = _textView.Caret.Position.BufferPosition;
                    var snapshot = _textView.TextBuffer.CurrentSnapshot;

                    if (viewCaretPosition < snapshot.Length)
                    {
                        char nextChar = snapshot.GetText(viewCaretPosition, 1)[0];
                        if (_typedChar == nextChar)
                        {
                            return;
                        }
                    }

                }

                OnPostTypeChar(_typedChar);
                _typedChar = '\0';
            }
        }

        void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (!IsActiveController || _processing)
                return;

            _typedChar = '\0';

            if (e.Changes.Count == 1 && e.AfterVersion.ReiteratedVersionNumber > _bufferVersionWaterline &&
                e.Changes[0].OldLength == 0 && e.Changes[0].NewLength >= 1)
            {
                var change = e.Changes[0];

                _bufferVersionWaterline = e.AfterVersion.ReiteratedVersionNumber;

                // Change length may be > 1 in autoformatting languages.
                // However, there will be only one non-ws character in the change.
                // Be careful when </script> is inserted: the change won't
                // actually be in this buffer.

                var snapshot = _textBuffer.CurrentSnapshot;
                if (change.NewSpan.End <= snapshot.Length)
                {
                    char typedChar = ProvisionalText.GetOneTypedCharacter(_textBuffer.CurrentSnapshot, change.NewSpan);

                    if (typedChar != '\0')
                    {
                        // Allow completion of different characters inside spans, but not when
                        // character and its completion pair is the same. For example, we do
                        // want to complete () in foo(bar|) when user types ( after bar. However,
                        // we do not want to complete " when user is typing in a string which
                        // was already completed and instead " should be a terminating type-through.

                        var completionChar = GetCompletionCharacter(typedChar);
                        var caretPosition = GetCaretPositionInBuffer();

                        if (caretPosition.HasValue)
                        {
                            bool compatible = true;

                            var innerText = GetInnerProvisionalText();
                            if (innerText != null)
                                compatible = IsCompatibleCharacter(innerText.ProvisionalChar, typedChar);

                            if (!IsPositionInProvisionalText(caretPosition.Value) || typedChar != completionChar || compatible)
                            {
                                _typedChar = typedChar;
                                _caretPosition = caretPosition.Value;
                            }
                        }
                    }
                }
            }
        }

        protected virtual char GetCompletionCharacter(char typedCharacter)
        {
            switch (typedCharacter)
            {
                case '\"':
                case '\'':
                    return typedCharacter;

                case '[':
                    return ']';

                case '(':
                    return ')';

                case '{':
                    return '}';
            }

            return '\0';
        }

        protected virtual bool IsCompatibleCharacter(char primaryCharacter, char candidateCharacter)
        {
            if (primaryCharacter == '\"' || primaryCharacter == '\'')
                return false; // no completion in strings

            return true;
        }

        private void OnPostTypeChar(char typedCharacter)
        {
            // When language autoformats, like JS, caret may be in a very different
            // place by now. Check if store caret position still makes sense and
            // if not, reacquire it. In contained language scenario
            // current caret position may be beyond projection boundary like when
            // typing at the end of onclick="return foo(".

            var settings = EditorShell.GetSettings(_textBuffer.ContentType.TypeName);
            if (settings != null && settings.GetBoolean(CommonSettings.InsertMatchingBracesKey, true))
            {
                char completionCharacter = GetCompletionCharacter(typedCharacter);
                if (completionCharacter != '\0')
                {
                    SnapshotPoint viewCaretPosition = _textView.Caret.Position.BufferPosition;
                    _processing = true;

                    SnapshotPoint? bufferCaretPosition = GetCaretPositionInBuffer();
                    if (bufferCaretPosition.HasValue)
                    {
                        _caretPosition = bufferCaretPosition.Value;
                    }
                    else if (viewCaretPosition.Position == _textView.TextBuffer.CurrentSnapshot.Length)
                    {
                        _caretPosition = _textBuffer.CurrentSnapshot.Length;
                    }

                    bool canComplete = TextViewHelpers.IsAutoInsertAllowed(_textView);

                    // Check the character before the caret
                    if (canComplete && _caretPosition > 0)
                    {
                        canComplete = CanCompleteAfter(_textBuffer, _caretPosition - 1, typedCharacter);
                    }

                    // Check the character after the caret
                    if (canComplete && _caretPosition < _textBuffer.CurrentSnapshot.Length)
                    {
                        canComplete = CanCompleteBefore(_textBuffer, _caretPosition, typedCharacter);
                    }

                    if (canComplete)
                    {
                        ProvisionalText.IgnoreChange = true;
                        _textView.TextBuffer.Replace(new Span(viewCaretPosition, 0), completionCharacter.ToString());
                        ProvisionalText.IgnoreChange = false;

                        _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, viewCaretPosition));

                        CreateProvisionalText(new Span(viewCaretPosition - 1, 2));
                    }
                }

                _processing = false;
            }
        }

        /// <summary>
        /// The span needs to end after the closing character (like a close brace)
        /// </summary>
        public void CreateProvisionalText(Span span)
        {
            ProvisionalText provisionalText = NewProvisionalText(_textView, span);
            provisionalText.Closing += new System.EventHandler<System.EventArgs>(OnCloseProvisionalText);

            _provisionalTexts.Add(provisionalText);
        }

        protected virtual ProvisionalText NewProvisionalText(ITextView textView, Span span)
        {
            return new ProvisionalText(textView, span);
        }

        private SnapshotPoint? GetCaretPositionInBuffer()
        {
            var viewCaretPosition = _textView.Caret.Position.BufferPosition.Position;
            var snapshot = _textView.TextBuffer.CurrentSnapshot;

            if (viewCaretPosition > snapshot.Length)
                return null;

            return _textView.BufferGraph.MapDownToBuffer(
                    new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, viewCaretPosition), PointTrackingMode.Positive,
                    _textBuffer, PositionAffinity.Predecessor);
        }

        private bool IsPositionInProvisionalText(int position)
        {
            foreach (var pt in _provisionalTexts)
            {
                if (pt.IsPositionInSpan(position))
                    return true;
            }

            return false;
        }

        private ProvisionalText GetInnerProvisionalText()
        {
            int minLength = Int32.MaxValue;
            ProvisionalText innerText = null;

            foreach (var pt in _provisionalTexts)
            {
                if (pt.CurrentSpan.Length < minLength)
                {
                    minLength = pt.CurrentSpan.Length;
                    innerText = pt;
                }
            }

            return innerText;
        }

        private void OnCloseProvisionalText(object sender, EventArgs e)
        {
            _provisionalTexts.Remove(sender as ProvisionalText);
        }

        #region IIntellisenseController Members

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            if (_textBuffer == subjectBuffer)
            {
                _connected = true;
                _textBuffer.Changed += TextBuffer_Changed;
                _textBuffer.PostChanged += TextBuffer_PostChanged;

                ServiceManager.AddService<TypeThroughController>(this, _textView);
            }
        }

        public void Detach(ITextView textView)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            if ((_textBuffer == subjectBuffer) && _connected)
            {
                TypeThroughController existingController = ServiceManager.GetService<TypeThroughController>(_textView);

                _connected = false;
                _textBuffer.Changed -= TextBuffer_Changed;
                _textBuffer.PostChanged -= TextBuffer_PostChanged;

                Debug.Assert(existingController == this);
                if (existingController == this)
                {
                    ServiceManager.RemoveService<TypeThroughController>(_textView);
                }
            }
        }
        #endregion
    }
}
