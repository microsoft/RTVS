// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Formatting {
    public sealed class TextBuilder {
        private readonly StringBuilder _formattedText = new StringBuilder();

        public TextBuilder(IndentBuilder indentBuilder) {
            IndentBuilder = indentBuilder;
        }

        public string Text => _formattedText.ToString();
        public int CurrentLineLength { get; private set; }
        public int CurrentIndent { get; private set; }
        public IndentBuilder IndentBuilder { get; }

        public string LineBreak { get; set; }

        public bool IsAtNewLine => CurrentLineLength == 0;

        public int CopyFollowingLineBreaks(ITextIterator iterator, int position) {
            var lineBreakCount = Whitespace.LineBreaksAfterPosition(iterator, position);
            var recentlyAddedCount = RecentlyAddedLineBreakCount();
            var breaks = lineBreakCount - recentlyAddedCount;

            for (var i = 0; i < breaks; i++) {
                HardLineBreak();
            }

            return breaks;
        }

        public int CopyPrecedingLineBreaks(ITextIterator iterator, int position) {
            var lineBreakCount = Whitespace.LineBreaksBeforePosition(iterator, position);
            var recentlyAddedCount = RecentlyAddedLineBreakCount();
            var breaks = lineBreakCount - recentlyAddedCount;

            for (var i = 0; i < breaks; i++) {
                HardLineBreak();
            }

            return breaks;
        }

        public void HardLineBreak() => AppendNewLine(true, true);
        public void SoftLineBreak() => AppendNewLine(true, false);
        private int RecentlyAddedLineBreakCount()
            => Whitespace.LineBreaksBeforePosition(new StringBuilderIterator(_formattedText), _formattedText.Length);

        private void AppendNewLine(bool collapseWhitespace = true, bool forceAdd = false) {
            if (collapseWhitespace) {
                TrimTrailingSpaces();
            }

            // Do not insert new line if it is there already
            if (!IsAtNewLine || forceAdd) {
                foreach (var ch in LineBreak) {
                    AppendText(ch);
                }
                CurrentLineLength = 0;
                CurrentIndent = 0;
            }
        }

        // Trim trailing spaces at the end of the existing text in the builder
        // does not affect indentation
        private void TrimTrailingSpaces() {
            int i;
            for (i = _formattedText.Length - 1; i >= 0; i--) {
                var ch = _formattedText[i];

                if (ch != ' ' && ch != '\t' && ch != 0x200B) {
                    break;
                }
            }

            var count = _formattedText.Length - i - 1;
            _formattedText.Remove(i + 1, count);
            CurrentLineLength -= count;
        }

        /// <summary>
        /// Inserts indentation whitespace according to the current indentation level.
        /// Does nothing if line already contains text (current line length is greater than 0).
        /// </summary>
        /// <returns>True of indentation text was inserted</returns>
        public bool SoftIndent() {
            if (char.IsWhiteSpace(LastCharacter)) {
                var allWhitespace = true;

                int i;
                for (i = _formattedText.Length - 1; i >= 0; i--) {
                    var ch = _formattedText[i];

                    if (ch.IsLineBreak()) {
                        break;
                    }

                    if (!char.IsWhiteSpace(ch)) {
                        allWhitespace = false;
                        break;
                    }
                }

                if (allWhitespace) {
                    _formattedText.Remove(i + 1, _formattedText.Length - i - 1);
                    CurrentLineLength = 0;
                    CurrentIndent = 0;
                }
            }

            if (IsAtNewLine) {
                AppendText(IndentBuilder.IndentLevelString);
                CurrentIndent = IndentBuilder.IndentLevelSize;
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

            for (var i = 0; i < words.Length; i++) {
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
                } else if (!char.IsWhiteSpace(LastCharacter)) {
                    AppendSpace();
                }
            }
        }


        public void NewIndentLevel() => IndentBuilder.NewIndentLevel();

        public void CloseIndentLevel() => IndentBuilder.CloseIndentLevel();

        public void AppendText(string text) {
            Debug.Assert(text.IndexOfAny(CharExtensions.LineBreakChars) < 0, "AppendText only accepts texts without line breaks. Use AppendPreformattedText instead");

            if (CurrentLineLength == 0 && !string.IsNullOrWhiteSpace(text)) {
                SoftIndent();
            }

            _formattedText.Append(text);
            CurrentLineLength += text.Length;
        }

        public void AppendPreformattedText(string text) {
            for (var i = 0; i < text.Length; i++) {
                var ch = text[i];
                if (ch.IsLineBreak()) {
                    AppendNewLine();
                } else {
                    AppendText(ch);
                }
            }
        }

        public void AppendText(char ch) {
            if (CurrentLineLength == 0 && !char.IsWhiteSpace(ch)) {
                SoftIndent();
            }

            _formattedText.Append(ch);
            CurrentLineLength++;
        }

        public void AppendSpace() {
            if (CurrentLineLength > 0 && !char.IsWhiteSpace(LastCharacter)) {
                AppendText(' ');
            }
        }

        public void Remove(int start, int length) => _formattedText.Remove(start, length);

        public char LastCharacter => _formattedText.Length > 0 ? _formattedText[_formattedText.Length - 1] : '\0';

        public int Length => _formattedText.Length;
    }
}
