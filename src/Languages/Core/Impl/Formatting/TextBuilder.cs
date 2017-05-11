// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Formatting {
    public sealed class TextBuilder {
        private StringBuilder _formattedText = new StringBuilder();
        private IndentBuilder _indentBuilder;

        private int _currentLineLength;
        private int _currentIndent;
        private int _currentLine;

        public TextBuilder(IndentBuilder indentBuilder) {
            _indentBuilder = indentBuilder;
        }

        public string Text { get { return _formattedText.ToString(); } }

        public int CurrentLineLength { get { return _currentLineLength; } }

        public int CurrentIndent { get { return _currentIndent; } }

        public IndentBuilder IndentBuilder { get { return _indentBuilder; } }

        public string LineBreak { get; set; }

        public bool IsAtNewLine { get { return _currentLineLength == 0; } }

        public int CopyFollowingLineBreaks(ITextIterator iterator, int position) {
            int lineBreakCount = Whitespace.LineBreaksAfterPosition(iterator, position);
            int recentlyAddedCount = RecentlyAddedLineBreakCount();
            int breaks = lineBreakCount - recentlyAddedCount;

            for (int i = 0; i < breaks; i++) {
                HardLineBreak();
            }

            return breaks;
        }

        public int CopyPrecedingLineBreaks(ITextIterator iterator, int position) {
            int lineBreakCount = Whitespace.LineBreaksBeforePosition(iterator, position);
            int recentlyAddedCount = RecentlyAddedLineBreakCount();
            int breaks = lineBreakCount - recentlyAddedCount;

            for (int i = 0; i < breaks; i++) {
                HardLineBreak();
            }

            return breaks;
        }

        public void HardLineBreak() {
            AppendNewLine(true, true);
        }

        public void SoftLineBreak() {
            AppendNewLine(true, false);
        }

        private int RecentlyAddedLineBreakCount() {
            return Whitespace.LineBreaksBeforePosition(new StringBuilderIterator(_formattedText), _formattedText.Length);
        }

        private void AppendNewLine(bool collapseWhitespace = true, bool forceAdd = false) {
            if (collapseWhitespace) {
                TrimTrailingSpaces();
            }

            // Do not insert new line if it is there already
            if (!IsAtNewLine || forceAdd) {
                foreach(var ch in LineBreak) {
                    AppendText(ch);
                }
                _currentLine++;
                _currentLineLength = 0;
                _currentIndent = 0;
            }
        }

        // Trim trailing spaces at the end of the existing text in the builder
        // does not affect indentation
        private void TrimTrailingSpaces() {
            int i;
            for (i = _formattedText.Length - 1; i >= 0; i--) {
                char ch = _formattedText[i];

                if (ch != ' ' && ch != '\t' && ch != 0x200B) {
                    break;
                }
            }

            int count = _formattedText.Length - i - 1;
            _formattedText.Remove(i + 1, count);
            _currentLineLength -= count;
        }

        /// <summary>
        /// Inserts indentation whitespace according to the current indentation level.
        /// Does nothing if line already contains text (current line length is greater than 0).
        /// </summary>
        /// <returns>True of indentation text was inserted</returns>
        public bool SoftIndent() {
            if (Char.IsWhiteSpace(LastCharacter)) {
                bool allWhitespace = true;

                int i;
                for (i = _formattedText.Length - 1; i >= 0; i--) {
                    char ch = _formattedText[i];

                    if (ch.IsLineBreak()) {
                        break;
                    }

                    if (!Char.IsWhiteSpace(ch)) {
                        allWhitespace = false;
                        break;
                    }
                }

                if (allWhitespace) {
                    _formattedText.Remove(i + 1, _formattedText.Length - i - 1);
                    _currentLineLength = 0;
                    _currentIndent = 0;
                }
            }

            if (IsAtNewLine) {
                AppendText(_indentBuilder.IndentLevelString);
                _currentIndent = _indentBuilder.IndentLevelSize;
                return true;
            }

            return false;
        }

        public void AppendTextWithWrap(string text, int wrapLength) {
            if (text.Length == 0) {
                return;
            }

            SoftIndent();

            // Split text into words at whitespace
            var words = text.Split(new char[] { ' ', '\t', '\r', '\n' });

            for (int i = 0; i < words.Length; i++) {
                var word = words[i];

                if (word.Length > 0) {
                    if (CurrentLineLength + word.Length > wrapLength) {
                        AppendNewLine();
                        SoftIndent();
                    }

                    AppendText(word);

                    if (i < words.Length - 1) {
                        AppendSpace();
                    }
                } else if (!Char.IsWhiteSpace(LastCharacter)) {
                    AppendSpace();
                }
            }
        }


        public void NewIndentLevel() {
            _indentBuilder.NewIndentLevel();
        }

        public void CloseIndentLevel() {
            _indentBuilder.CloseIndentLevel();
        }

        public void AppendText(string text) {
            Debug.Assert(text.IndexOfAny(CharExtensions.LineBreakChars) < 0, "AppendText only accepts texts without line breaks. Use AppendPreformattedText instead");

            if (_currentLineLength == 0 && !string.IsNullOrWhiteSpace(text)) {
                SoftIndent();
            }

            _formattedText.Append(text);
            _currentLineLength += text.Length;
        }

        public void AppendPreformattedText(string text) {
            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];
                if (ch.IsLineBreak()) {
                    AppendNewLine();
                } else {
                    AppendText(ch);
                }
            }
        }

        public void AppendText(char ch) {
            if (_currentLineLength == 0 && !char.IsWhiteSpace(ch)) {
                SoftIndent();
            }

            _formattedText.Append(ch);
            _currentLineLength++;
        }

        public void AppendSpace() {
            if (_currentLineLength > 0 && !char.IsWhiteSpace(LastCharacter)) {
                AppendText(' ');
            }
        }

        public void Remove(int start, int length) {
            _formattedText.Remove(start, length);
        }

        public char LastCharacter {
            get {
                return _formattedText.Length > 0 ? _formattedText[_formattedText.Length - 1] : '\0';
            }
        }

        public int Length {
            get { return _formattedText.Length; }
        }
    }
}
