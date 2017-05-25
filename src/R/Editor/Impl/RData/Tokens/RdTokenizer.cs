// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.RData.Tokens {
    /// <summary>
    /// Main R tokenizer. Used for colirization and parsing. 
    /// Coloring of variables, function names and parameters
    /// is provided later by AST. Tokenizer only provides candidates.
    /// https://developer.r-project.org/parseRd.pdf
    /// </summary>
    public class RdTokenizer : BaseTokenizer<RdToken> {
        private readonly bool _tokenizeRContent;
        private BlockContentType _currentContentType = BlockContentType.Latex;

        public RdTokenizer() :
            this(tokenizeRContent: true) {
        }

        /// <summary>
        /// Creates RD tokenizer.
        /// </summary>
        /// <param name="tokenizeRContent">
        /// If true, RD tokenizer will use R rokenizer to process 
        /// content in R-like sections such as \usage. This is 
        /// the default behavior since it allows colorizer to
        /// properly display strings and numbers. However, during
        /// processing of RD data for the function signature help
        /// and quick info tooltips plain text is preferred.
        /// </param>
        public RdTokenizer(bool tokenizeRContent = true) {
            _tokenizeRContent = tokenizeRContent;
        }

        public override IReadOnlyTextRangeCollection<RdToken> Tokenize(ITextProvider textProvider, int start, int length, bool excludePartialTokens) {
            _currentContentType = BlockContentType.Latex;
            return base.Tokenize(textProvider, start, length, excludePartialTokens);
        }

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

            HandleLatexContent(block: false);
        }

        private void HandleLatexContent(bool block) {
            var braceCounter = block ? new BraceCounter<char>(new char[] { '{', '}', '[', ']' }) : null;

            while (!_cs.IsEndOfStream()) {
                var handled = false;

                // Regular content is Latex-like
                switch (_cs.CurrentChar) {
                    case '%':
                        handled = HandleComment();
                        break;

                    case '\\':
                        if (IsEscape()) {
                            _cs.Advance(2);
                            handled = true;
                        } else {
                            handled = HandleKeyword();
                        }
                        break;

                    case '#':
                        handled = HandlePragma();
                        break;

                    default:
                        if (braceCounter != null && braceCounter.CountBrace(_cs.CurrentChar)) {
                            handled = AddBraceToken();

                            if (braceCounter.Count == 0) {
                                return;
                            }
                        }
                        break;
                }

                if (!handled) {
                    _cs.MoveToNextChar();
                }
            }
        }

        private bool HandleKeyword() {
            var start = _cs.Position;

            if (MoveToKeywordEnd()) {
                AddToken(RdTokenType.Keyword, start, _cs.Position - start);
                SkipWhitespace();

                if (_cs.CurrentChar == '{' || _cs.CurrentChar == '[') {
                    var keyword = _cs.Text.GetText(TextRange.FromBounds(start, _cs.Position)).Trim();
                    var contentType = RdBlockContentType.GetBlockContentType(keyword);

                    if (_currentContentType != contentType) {
                        _currentContentType = contentType;

                        Debug.Assert(_tokens[_tokens.Count - 1].TokenType == RdTokenType.Keyword);
                        _tokens[_tokens.Count - 1].ContentTypeChange = true;
                    }

                    // Handle argument sequence like \latex[0]{foo} or \item{}{}
                    while (_cs.CurrentChar == '{' || _cs.CurrentChar == '[') {
                        HandleKeywordArguments(contentType);
                    }
                }

                return true;
            }

            return false;
        }

        private bool MoveToKeywordEnd() {
            var start = _cs.Position;

            _cs.MoveToNextChar();
            SkipKeyword();

            if (_cs.Position - start > 1) {
                return true;
            }

            _cs.Position = start;
            return false;
        }

        private void HandleKeywordArguments(BlockContentType contentType) {
            // Content type table can be found in 
            // https://developer.r-project.org/parseRd.pdf

            switch (contentType) {
                case BlockContentType.R:
                    HandleRContent();
                    break;

                case BlockContentType.Verbatim:
                    HandleVerbatimContent();
                    break;

                default:
                    HandleLatexContent(block: true);
                    break;
            }
        }

        /// <summary>
        /// Handles R-like content in RD. This includes handling # and ##
        /// as comments, counting brace nesting, handling "..." as string
        /// (not true in plain RD LaTeX-like content) and colorizing numbers
        /// by using actual R tokenizer. Now. there is a confusing part:
        /// "There are two types of comments in R-like mode. As elsewhere in 
        /// Rd ﬁles, Rd comments start with %, and run to the end of the line."
        /// If that is so then $ in sprintf will beging RD comment which frankly
        /// doesn't make any sense fron the authoring/editing point of view.
        /// "% characters must be escaped even within strings, or they will be 
        /// taken as Rd comments." Sure, but R engine doesn't do that when 
        /// requesting help in Rd format.
        /// </summary>
        private void HandleRContent() {
            var braceCounter = new BraceCounter<char>(new [] { '{', '}', '[', ']' });

            while (!_cs.IsEndOfStream()) {
                var handled = false;
                switch (_cs.CurrentChar) {
                    case '\"':
                    case '\'':
                        handled = HandleRString(_cs.CurrentChar);
                        break;

                    case '\\':
                        handled = IsEscape();
                        if (handled) {
                            _cs.Advance(2);
                        } else {
                            handled = HandleKeyword();
                        }
                        break;

                    case '#':
                        handled = HandlePragma();
                        if (!handled) {
                            if (_cs.NextChar == '#') {
                                // ## is always comment in R-like content
                                handled = HandleComment();
                            } else {
                                // With a sinle # it may or may not be comment.
                                // For example, there are statements like \code{#}.
                                // Heuristic is to treat text that contains {} or \
                                // as NOT a comment.
                                var commentStart = _cs.Position;
                                _cs.SkipToEol();

                                var commentText = _cs.Text.GetText(TextRange.FromBounds(commentStart, _cs.Position));
                                _cs.Position = commentStart;

                                if (commentText.IndexOfAny(new[] { '{', '\\', '}' }) < 0) {
                                    handled = HandleComment();
                                }
                            }
                        }
                        break;

                    default:
                        if (braceCounter.CountBrace(_cs.CurrentChar)) {
                            handled = AddBraceToken();

                            if (braceCounter.Count == 0) {
                                return;
                            }
                        } else {
                            // Check if sequence is a candidate for a number.
                            // The check is not perfect but numbers in R-like content
                            // are typically very simple as R blocks are usually
                            // code examples and do not contain exotic sequences.

                            if (!char.IsLetter(_cs.PrevChar) && (_cs.IsDecimal() || _cs.CurrentChar == '-' || _cs.CurrentChar == '.')) {
                                var sequenceStart = _cs.Position;
                                _cs.SkipToWhitespace();

                                if (_cs.Position > sequenceStart) {
                                    var rt = new RTokenizer();

                                    var candidate = _cs.Text.GetText(TextRange.FromBounds(sequenceStart, _cs.Position));
                                    var rTokens = rt.Tokenize(candidate);

                                    if (rTokens.Count > 0 && rTokens[0].TokenType == RTokenType.Number) {
                                        if (_tokenizeRContent) {
                                            AddToken(RdTokenType.Number, sequenceStart + rTokens[0].Start, rTokens[0].Length);
                                        }
                                        _cs.Position = sequenceStart + rTokens[0].End;
                                        continue;
                                    }
                                }

                                _cs.Position = sequenceStart;
                            }
                        }
                        break;
                }

                if (!handled) {
                    _cs.MoveToNextChar();
                }
            }
        }

        /// <summary>
        /// Handles verbatim text content where there are no special characters 
        /// apart from braces and pragmas. 
        /// 
        /// Verbatim text within an Rd ﬁle is a pure stream of text, uninterpreted 
        /// by the parser, with the exceptions that braces must balance or be escaped, 
        /// and % comments are recognized, and backslashes that could be interpreted 
        /// as escapes must themselves be escaped. No markup macros are recognized 
        /// within verbatim text.
        /// 
        /// OK, here is a problem. "Could be interpreted as escapes". Such as when? 
        /// What about % inside C-like printf formats? I think we just will ignore %
        /// and handle \ as keywords...
        /// 
        /// NOTE: since % is confusing and can be a C-like format specification
        /// in \examples{ } that as far as I can see don't get % escaped,
        /// we won't be really handling % as comments here,
        /// 
        /// https://developer.r-project.org/parseRd.pdf
        /// Verbatim text within an Rd ﬁle is a pure stream of text, uninterpreted 
        /// by the parser, with the exceptions that braces must balance or be escaped, 
        /// and % comments are recognized, and backslashes that could be interpreted 
        /// as escapes must themselves be escaped. 
        /// </summary>
        private void HandleVerbatimContent() {
            var braceCounter = new BraceCounter<char>(new[] { '{', '}', '[', ']' });

            while (!_cs.IsEndOfStream()) {
                var handled = false;

                switch (_cs.CurrentChar) {
                    case '\\':
                        handled = IsEscape();
                        if (handled) {
                            _cs.Advance(2);
                        } else {
                            handled = HandleKeyword();
                        }
                        break;

                    case '%':
                        // In 'verbatim' text we handke % as comment
                        // when it is in the beginning of the file
                        if (_cs.Position == 0 || _cs.PrevChar == '\r' || _cs.PrevChar == '\n') {
                            handled = HandleComment();
                        }
                        break;

                    default:
                        if (braceCounter.CountBrace(_cs.CurrentChar)) {
                            handled = AddBraceToken();

                            if (braceCounter.Count == 0) {
                                return;
                            }
                        } else if (_cs.CurrentChar == '#' && HandlePragma()) {
                            continue;
                        }
                        break;
                }

                if (!handled) {
                    _cs.MoveToNextChar();
                }
            }
        }

        /// <summary>
        /// Handles RD conditional pragmas (C-like).
        /// </summary>
        /// <returns></returns>
        private bool HandlePragma() {
            if (_cs.Position == 0 || _cs.PrevChar.IsLineBreak()) {
                var start = _cs.Position;
                SkipUnknown();

                var length = _cs.Position - start;
                if (length > 1) {
                    var pragma = _cs.Text.GetText(TextRange.FromBounds(start, _cs.Position)).Trim();
                    if (pragma == "#ifdef" || pragma == "#ifndef" || pragma == "#endif") {
                        AddToken(RdTokenType.Pragma, start, length);
                        return true;
                    }
                }

                _cs.Position = start;
            }

            return false;
        }

        private char GetMatchingBrace(char brace) {
            if (brace == '{') {
                return '}';
            }

            if (brace == '[') {
                return ']';
            }

            return char.MinValue;
        }

        private bool AddBraceToken() {
            var tokenType = RdTokenType.Unknown;

            switch (_cs.CurrentChar) {
                case '{':
                    tokenType = RdTokenType.OpenCurlyBrace;
                    break;

                case '[':
                    tokenType = RdTokenType.OpenSquareBracket;
                    break;

                case '}':
                    tokenType = RdTokenType.CloseCurlyBrace;
                    break;

                case ']':
                    tokenType = RdTokenType.CloseSquareBracket;
                    break;
            }

            if (tokenType != RdTokenType.Unknown) {
                AddToken(tokenType, _cs.Position, 1);
                _cs.MoveToNextChar();
                return true;

            }

            return false;
        }

        private bool IsEscape() {
            return _cs.NextChar == '%' || _cs.NextChar == '\\' || _cs.NextChar == '{' || _cs.NextChar == '}' || _cs.NextChar == 'R';
        }

        /// <summary>
        /// Handle RD comment. Comment starts with %
        /// and goes to the end of the line.
        /// </summary>
        private bool HandleComment() {
            Tokenizer.HandleEolComment(_cs, (start, length) => AddToken(RdTokenType.Comment, start, length));
            return true;
        }

        /// <summary>
        /// Adds a token that represent R string
        /// </summary>
        /// <param name="openQuote"></param>
        private bool HandleRString(char openQuote) {
            Tokenizer.HandleString(openQuote, _cs, (start, length) => {
                if (_tokenizeRContent) {
                    AddToken(RdTokenType.String, start, length);
                }
            });
            return true;
        }

        private void AddToken(RdTokenType type, int start, int length) {
            var token = new RdToken(type, new TextRange(start, length));
            _tokens.Add(token);
        }

        internal void SkipKeyword() {
            Tokenizer.SkipIdentifier(
                _cs,
                cs => _cs.IsLetter(),
                cs => _cs.IsLetter() || _cs.IsDecimal());
        }

        /// <summary>
        /// Skips content until the nearest whitespace
        /// or a RD comment that starts with %.
        /// </summary>
        internal void SkipUnknown() {
            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace()) {
                if (_cs.CurrentChar == '%') {
                    break;
                }

                _cs.MoveToNextChar();
            }
        }
    }
}
