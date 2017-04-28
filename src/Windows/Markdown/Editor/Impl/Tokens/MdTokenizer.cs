// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Markdown.Editor.Tokens {
    /// <summary>
    /// Main regular markdown tokenizer. R Markdown has 
    /// a separate tokenizer.
    /// https://help.github.com/articles/markdown-basics/
    /// </summary>
    internal class MdTokenizer : BaseTokenizer<MarkdownToken> {
        /// <summary>
        /// Main tokenization method. Responsible for adding next token
        /// to the list, if any. Returns if it is at the end of the 
        /// character stream. It is up to base class to terminate tokenization.
        /// </summary>
        public override void AddNextToken() {
            SkipWhitespace();

            if (_cs.IsEndOfStream()) {
                return;
            }

            HandleCharacter();
        }

        protected virtual void HandleCharacter() {
            while (!_cs.IsEndOfStream()) {
                bool handled = false;

                // Regular content is Latex-like
                switch (_cs.CurrentChar) {
                    case '#':
                        handled = HandleHeading();
                        break;

                    case '*':
                        handled = HandleStar();
                        break;

                    case '_':
                        if (!char.IsWhiteSpace(_cs.NextChar)) {
                            handled = HandleItalic('_', MarkdownTokenType.Italic);
                        }
                        break;

                    case '>':
                        handled = HandleQuote();
                        break;

                    case '`':
                        handled = HandleBackTick();
                        break;

                    case '-':
                        if (_cs.NextChar == ' ') {
                            handled = HandleListItem();
                        } else if (_cs.NextChar == '-' && _cs.LookAhead(2) == '-') {
                            handled = HandleHeading();
                        }
                        break;

                    case '=':
                        if (_cs.NextChar == '=' && _cs.LookAhead(2) == '=') {
                            handled = HandleHeading();
                        }
                        break;

                    case '[':
                        handled = HandleAltText();
                        break;

                    default:
                        if (_cs.IsDecimal()) {
                            handled = HandleNumberedListItem();
                        }
                        break;

                }

                if (!handled) {
                    _cs.MoveToNextChar();
                }
            }
        }

        protected bool HandleHeading() {
            if (_cs.Position == 0 || _cs.PrevChar.IsLineBreak()) {
                return HandleSequenceToEol(MarkdownTokenType.Heading);
            }

            return false;
        }

        protected bool HandleQuote() {
            if (_cs.Position == 0 || _cs.PrevChar.IsLineBreak()) {
                if (_cs.NextChar == ' ') {
                    return HandleSequenceToEmptyLine(MarkdownTokenType.Blockquote);
                }
            }

            return false;
        }

        protected bool HandleAltText() {
            int start = _cs.Position;
            while (!_cs.IsEndOfStream()) {
                if (_cs.CurrentChar == ']' || _cs.IsAtNewLine()) {
                    break;
                }

                _cs.MoveToNextChar();
            }

            if (_cs.CurrentChar == ']' && _cs.NextChar == '(') {
                int end = _cs.Position + 1;
                _cs.Advance(2);

                while (!_cs.IsEndOfStream()) {
                    if (_cs.CurrentChar == ')' || _cs.IsAtNewLine()) {
                        break;
                    }

                    _cs.MoveToNextChar();
                }

                if (_cs.CurrentChar == ')') {
                    AddToken(MarkdownTokenType.AltText, start, end - start);
                }
            }

            return true;
        }

        protected bool HandleBackTick() {
            if (_cs.NextChar == '`' && _cs.LookAhead(2) == '`' && (_cs.Position == 0 || _cs.PrevChar == '\n' || _cs.PrevChar == '\r')) {
                return HandleCode(block: true);
            }

            // If it is `r ... then it is inline R code
            if (_cs.NextChar == 'r' && char.IsWhiteSpace(_cs.LookAhead(2))) {
                return HandleCode(block: false);
            }

            return HandleMonospace();
        }

        protected bool HandleCode(bool block) {
            int ticksStart = _cs.Position;
            int leadingSeparatorLength;
            int trailingSeparatorLength;

            leadingSeparatorLength = block ? 3 : 1;
            _cs.Advance(leadingSeparatorLength);

            // block in R: '''{r qplot, x=y, ...}
            bool rLanguage = block && (_cs.CurrentChar == '{' && (_cs.NextChar == 'r' || _cs.NextChar == 'R'));
            bool rInline = !block && (_cs.CurrentChar == 'r' || _cs.CurrentChar == 'R');
            if (rInline) {
                leadingSeparatorLength = 2; // include 'R'
            }
            rLanguage |= rInline;

            // Move past {
            _cs.MoveToNextChar();
            while (!_cs.IsEndOfStream()) {
                // End of R block: <line_break>```
                bool endOfBlock = block && _cs.IsAtNewLine() && _cs.NextChar == '`' && _cs.LookAhead(2) == '`' && _cs.LookAhead(3) == '`';
                bool endOfInline = !block && _cs.CurrentChar == '`';
                bool eof = _cs.Position == _cs.Length - 1; // handle unclosed code block as if it ends at EOF

                if (endOfBlock) {
                    _cs.SkipLineBreak();
                    trailingSeparatorLength = 3;
                } else if (endOfInline) {
                    // inline code `r 1 + 1`
                    trailingSeparatorLength = 1;
                } else { // eof
                    trailingSeparatorLength = 0;
                }

                if (endOfBlock || endOfInline || eof) {
                    if (eof) {
                        // At the end of the file current position if one less. 
                        // since end is not included, we want end to be at the buffer length;
                        _cs.MoveToNextChar();
                    } else {
                        _cs.Advance(trailingSeparatorLength); // past the end of block now
                    }

                    // Opening ticks
                    if (rLanguage) {
                        AddCodeToken(ticksStart, _cs.Position - ticksStart, leadingSeparatorLength, trailingSeparatorLength);
                    } else {
                        AddToken(MarkdownTokenType.Monospace, ticksStart, _cs.Position - ticksStart);
                    }
                    return true;
                }
                _cs.MoveToNextChar();
            }
            return false;
        }

        protected bool HandleMonospace() {
            int start = _cs.Position;
            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream()) {
                if (_cs.CurrentChar == '`') {
                    _cs.MoveToNextChar();
                    AddToken(MarkdownTokenType.Monospace, start, _cs.Position - start);
                    return true;
                }

                _cs.MoveToNextChar();
            }

            return false;
        }

        protected bool HandleStar() {
            int start = _cs.Position;

            switch (_cs.NextChar) {
                case '*':
                    if (!char.IsWhiteSpace(_cs.LookAhead(2))) {
                        return HandleBold(MarkdownTokenType.Bold);
                    }
                    break;

                case ' ':
                    return HandleListItem();

                default:
                    if (!char.IsWhiteSpace(_cs.NextChar)) {
                        return HandleItalic('*', MarkdownTokenType.Italic);
                    }
                    break;
            }

            return false;
        }

        protected bool HandleBold(MarkdownTokenType tokenType) {
            int start = _cs.Position;

            _cs.Advance(2);
            while (!_cs.IsEndOfStream()) {
                if (_cs.CurrentChar == '_' || (_cs.CurrentChar == '*' && _cs.NextChar != '*')) {
                    int tokenCount = _tokens.Count;
                    AddToken(tokenType, start, _cs.Position - start);

                    int startOfItalic = _cs.Position;
                    if (HandleItalic(_cs.CurrentChar, MarkdownTokenType.BoldItalic)) {
                        start = _cs.Position;
                    } else {
                        _tokens.RemoveRange(tokenCount, _tokens.Count - tokenCount);
                        _cs.Position = startOfItalic;
                        break;
                    }
                }

                if (_cs.CurrentChar == '*' && _cs.NextChar == '*') {
                    _cs.Advance(2);
                    AddToken(tokenType, start, _cs.Position - start);
                    return true;
                }

                if (_cs.IsAtNewLine()) {
                    break;
                }

                _cs.MoveToNextChar();
            }

            return false;
        }

        protected bool HandleItalic(char boundaryChar, MarkdownTokenType tokenType) {
            int start = _cs.Position;

            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream()) {
                if (_cs.CurrentChar == '*' && _cs.NextChar == '*') {
                    int tokenCount = _tokens.Count;
                    AddToken(tokenType, start, _cs.Position - start);

                    int startOfBold = _cs.Position;
                    if (HandleBold(MarkdownTokenType.BoldItalic)) {
                        start = _cs.Position;
                    } else {
                        _tokens.RemoveRange(tokenCount, _tokens.Count - tokenCount);
                        _cs.Position = startOfBold;
                        break;
                    }
                }

                if (_cs.CurrentChar == boundaryChar) {
                    _cs.MoveToNextChar();
                    AddToken(tokenType, start, _cs.Position - start);
                    return true;
                }

                if (_cs.IsAtNewLine()) {
                    break;
                }

                _cs.MoveToNextChar();
            }

            return false;
        }

        protected bool HandleListItem() {
            // List item must start at the beginning of the line
            bool atStartOfLine = _cs.Position == 0;

            if (!atStartOfLine) {
                for (int i = _cs.Position - 1; i >= 0; i--) {
                    char ch = _cs[i];

                    if (!char.IsWhiteSpace(ch)) {
                        break;
                    }

                    if (ch.IsLineBreak()) {
                        atStartOfLine = true;
                        break;
                    }
                }
            }

            if (atStartOfLine) {
                return HandleSequenceToEol(MarkdownTokenType.ListItem);
            }

            return false;
        }

        protected bool HandleNumberedListItem() {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream()) {
                if (!_cs.IsDecimal()) {
                    if (_cs.CurrentChar == '.' && char.IsWhiteSpace(_cs.NextChar)) {
                        _cs.Position = start;
                        return HandleListItem();
                    }
                    break;
                }
                _cs.MoveToNextChar();
            }

            return false;
        }

        protected bool HandleSequenceToEol(MarkdownTokenType tokeType, int startPosition = -1) {
            int start = startPosition >= 0 ? startPosition : _cs.Position;
            _cs.SkipToEol();

            AddToken(tokeType, start, _cs.Position - start);
            return true;
        }

        protected bool HandleSequenceToEmptyLine(MarkdownTokenType tokeType) {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream()) {
                _cs.SkipToEol();
                _cs.SkipLineBreak();

                if (_cs.IsAtNewLine()) {
                    break;
                }
            }

            AddToken(tokeType, start, _cs.Position - start);
            return true;
        }

        protected void AddToken(MarkdownTokenType type, int start, int length) {
            Debug.Assert(type != MarkdownTokenType.Code, "Use AddCodeToken instead");
            if (length > 0) {
                var token = new MarkdownToken(type, new TextRange(start, length));
                _tokens.Add(token);
            }
        }

        private void AddCodeToken(int start, int length, int leadingSeparatorLength, int trailingSeparatorLength) {
            if (length > 0) {
                var token = new MarkdownCodeToken(new TextRange(start, length), leadingSeparatorLength, trailingSeparatorLength);
                _tokens.Add(token);
            }
        }

        /// <summary>
        /// Skips content until the nearest whitespace
        /// </summary>
        internal void SkipToWhitespace() {
            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace()) {
                _cs.MoveToNextChar();
            }
        }
    }
}
