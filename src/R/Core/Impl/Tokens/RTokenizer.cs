// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Core.Tokens {
    /// <summary>
    /// Main R tokenizer. Used for colirization and parsing. 
    /// Coloring of variables, function names and parameters
    /// is provided later by AST. Tokenizer only provides candidates.
    /// </summary>
    public sealed class RTokenizer : BaseTokenizer<RToken> {
        private readonly Stack<RTokenType> _squareBraceScope = new Stack<RTokenType>();
        private readonly List<RToken> _comments = new List<RToken>();
        private readonly bool _separateComments;

        public IReadOnlyList<RToken> CommentTokens => _comments;

        public RTokenizer() : this(false) { }

        public RTokenizer(bool separateComments = false) {
            // R parser needs comments separately
            // while colorizer all in one collection.
            _separateComments = separateComments;
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

            // First look at the numbers. Note that it is hard to tell
            // 12 +1 if it is a sum of numbers or a sequence. Note that
            // R also supports complex numbers like 1.e+01+-37.5i
            if (IsPossibleNumber()) {
                var start = _cs.Position;

                var length = NumberTokenizer.HandleNumber(_cs);
                if (length > 0) {
                    // HandleNumber will stop at 'i' as it will stop
                    // at any other non-digit (decimal or hex). Also,
                    // it doesn't know if + or - is an operator or part
                    // of the complex number.
                    HandleNumber(start, length);
                    return;
                }
            }

            HandleCharacter();
        }

        private void HandleCharacter() {
            switch (_cs.CurrentChar) {
                case '\"':
                case '\'':
                    HandleString(_cs.CurrentChar);
                    break;

                case '#':
                    // R Comments are from # to the end of the line
                    HandleComment();
                    break;

                case '(':
                    AddToken(RTokenType.OpenBrace, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case ')':
                    AddToken(RTokenType.CloseBrace, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case '[':
                    HandleOpenSquareBracket();
                    break;

                case ']':
                    HandleCloseSquareBracket();
                    break;

                case '{':
                    AddToken(RTokenType.OpenCurlyBrace, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case '}':
                    AddToken(RTokenType.CloseCurlyBrace, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case ',':
                    AddToken(RTokenType.Comma, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case ';':
                    AddToken(RTokenType.Semicolon, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                default:
                    if (_cs.CurrentChar == '.' && _cs.NextChar == '.' && _cs.LookAhead(2) == '.') {
                        AddToken(RTokenType.Ellipsis, _cs.Position, 3);
                        _cs.Advance(3);
                    } else if (_cs.CurrentChar == '=' && _cs.NextChar != '=') {
                        AddToken(RTokenType.Operator, _cs.Position, 1);
                        _cs.MoveToNextChar();
                    } else {
                        HandleOther();
                    }
                    break;
            }
        }

        internal bool IsPossibleNumber() {
            // It is hard to tell in 12 +1 if it is a sum of numbers or
            // a sequence. If operator or punctiation (comma, semicolon)
            // precedes the sign then sign is part of the number. 
            // Note that if preceding token is one of the function () 
            // or indexing braces [] then sign is an operator like in x[1]+2.
            // In other cases plus or minus is also a start of the operator. 
            // It important that in partial tokenization classifier removes
            // enough tokens so tokenizer can start its work early enough 
            // in the stream to be able to figure out numbers properly.

            if (_cs.CurrentChar == '-' || _cs.CurrentChar == '+') {
                // Next character must be decimal or a dot otherwise
                // it is not a number. No whitespace is allowed.
                if (CharacterStream.IsDecimal(_cs.NextChar) || _cs.NextChar == '.') {
                    // Check what previous token is, if any
                    if (_tokens.Count == 0) {
                        // At the start of the file this can only be a number
                        return true;
                    }

                    var previousToken = _tokens[_tokens.Count - 1];

                    if (previousToken.TokenType == RTokenType.OpenBrace ||
                        previousToken.TokenType == RTokenType.OpenSquareBracket ||
                        previousToken.TokenType == RTokenType.Comma ||
                        previousToken.TokenType == RTokenType.Semicolon ||
                        previousToken.TokenType == RTokenType.Operator) {
                        return true;
                    }
                }

                return false;
            }

            // R only supports 0xABCD. x0A is not legal.
            if (_cs.CurrentChar == '0' && _cs.NextChar == 'x') {
                // Hex humber like 0xA1BC
                return true;
            }

            if (_cs.IsDecimal()) {
                return true;
            }

            if (_cs.CurrentChar == '.' && CharacterStream.IsDecimal(_cs.NextChar)) {
                return true;
            }

            return false;
        }
        private void HandleNumber(int numberStart, int length) {
            if (_cs.CurrentChar == 'i') {
                _cs.MoveToNextChar();
                AddToken(RTokenType.Complex, numberStart, _cs.Position - numberStart);
                return;
            }

            // Check if this is actually complex number
            var imaginaryStart = _cs.Position;

            var imaginaryLength = NumberTokenizer.HandleImaginaryPart(_cs);
            if (imaginaryLength > 0) {
                AddToken(RTokenType.Complex, numberStart, length + imaginaryLength);
                return;
            }

            _cs.Position = imaginaryStart;
            AddToken(RTokenType.Number, numberStart, length);
        }

        private void HandleOpenSquareBracket() {
            RTokenType tokenType;
            int length;

            if (_cs.NextChar == '[') {
                tokenType = RTokenType.OpenDoubleSquareBracket;
                length = 2;
            } else {
                tokenType = RTokenType.OpenSquareBracket;
                length = 1;
            }

            AddToken(tokenType, _cs.Position, length);
            _cs.Advance(length);

            _squareBraceScope.Push(tokenType);
        }

        private void HandleCloseSquareBracket() {
            if (_cs.NextChar == ']') {
                // ]] candidate. We need to handle a[b[c]] 

                if (_squareBraceScope.Count > 0 && _squareBraceScope.Peek() == RTokenType.OpenDoubleSquareBracket) {
                    AddToken(RTokenType.CloseDoubleSquareBracket, _cs.Position, 2);
                    _cs.Advance(2);
                    _squareBraceScope.Pop();
                    return;
                }
            }

            AddToken(RTokenType.CloseSquareBracket, _cs.Position, 1);
            _cs.MoveToNextChar();

            if (_squareBraceScope.Count > 0 && _squareBraceScope.Peek() == RTokenType.OpenSquareBracket) {
                _squareBraceScope.Pop();
            }
        }

        private void HandleOther() {
            // Letter may be starting keyword, function or a variable name. 
            // At this point we should be either right after whitespace or 
            // at the beginning of the file.
            if (_cs.IsLetter() || _cs.CurrentChar == '.' || _cs.CurrentChar == '`') {
                // If this is not a keyword or a function name candidate
                HandleKeywordOrIdentifier();
                return;
            }

            // If character is not a letter and not start of a string it 
            // cannot be a keyword, function or variable name. Try operators 
            // first since they are longer than puctuation.
            if (HandleOperator()) {
                return;
            }

            // Something unknown. Skip to whitespace and file as unknown.
            // Note however, we should take # ito account as it starts comment
            var start = _cs.Position;
            if (Char.IsLetter(_cs.CurrentChar)) {
                if (AddIdentifier()) {
                    return;
                }
            }

            SkipUnknown();
            if (_cs.Position > start) {
                AddToken(RTokenType.Unknown, start, _cs.Position - start);
            } else {
                _cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Detemines if current position is at operator 
        /// and adds the appropriate token if so.
        /// </summary>
        /// <returns></returns>
        internal bool HandleOperator() {
            var length = Operators.OperatorLength(_cs);
            if (length > 0) {
                AddToken(RTokenType.Operator, _cs.Position, length);
                _cs.Advance(length);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if current position is at a keyword or 
        /// at a function name candidate. Function name candidate 
        /// looks like 'identifier[whitespace](' sequence.
        /// </summary>
        /// <returns>
        /// True if it handled the sequence and it looked 
        /// like either keyword or a function candidate.
        /// </returns>
        private void HandleKeywordOrIdentifier() {
            var start = _cs.Position;

            var s = this.GetIdentifier();
            if (s.Length == 0) {
                AddToken(RTokenType.Unknown, start, 1);
                _cs.MoveToNextChar();
                return;
            }

            if (s[0] == '`') {
                AddToken(RTokenType.Identifier, RTokenSubType.None, start, s.Length);
            } else if (Logicals.IsLogical(s)) {
                // Tell between F and F() and allow T = x or F <- 0.
                AddToken(VerifyLogical(s) ? RTokenType.Logical : RTokenType.Identifier, start, s.Length);
            } else if (s == "NULL") {
                AddToken(RTokenType.Null, RTokenSubType.BuiltinConstant, start, s.Length);
            } else if (s == "NA" || s == "NA_character_" || s == "NA_complex_" || s == "NA_integer_" || s == "NA_real_") {
                AddToken(RTokenType.Missing, RTokenSubType.BuiltinConstant, start, s.Length);
            } else if (s == "Inf") {
                AddToken(RTokenType.Infinity, RTokenSubType.BuiltinConstant, start, s.Length);
            } else if (s == "NaN") {
                AddToken(RTokenType.NaN, RTokenSubType.BuiltinConstant, start, s.Length);
            } else if (Builtins.IsBuiltin(s)) {
                AddToken(RTokenType.Identifier, RTokenSubType.BuiltinFunction, start, s.Length);
            } else if (s.StartsWith("as.", StringComparison.Ordinal) || s.StartsWith("is.", StringComparison.Ordinal)) {
                AddToken(RTokenType.Identifier, RTokenSubType.TypeFunction, start, s.Length);
            } else if (Keywords.IsKeyword(s)) {
                AddToken(RTokenType.Keyword, start, s.Length);
            } else {
                AddToken(RTokenType.Identifier, start, s.Length);
            }
        }

        /// <summary>
        /// Tries to determine if T/F are actually logical values.
        /// </summary>
        /// <remarks>This is not 100% reliable during tokenization since T or F may be 
        /// part of expression such as T &lt;- 0 and then, 100 lines later, `x = T + 2`. 
        /// However, in the latter case we can leave T/F as logical and let expression
        /// parser figure out the context.</remarks>
        private bool VerifyLogical(string s) {
            if (s.Length == 1) {
                if (_tokens.Count > 0) {
                    var prevToken = _tokens[_tokens.Count - 1];
                    if (prevToken.TokenType == RTokenType.Operator &&
                        _cs.GetSubstringAt(prevToken.Start, prevToken.Length).EqualsOrdinal("->")) {
                        // 1 -> T
                        return false;
                    }
                }
                _cs.SkipWhitespace();
                switch (_cs.CurrentChar) {
                    case '(': // F(
                    case '=': // T = 2
                        return false;
                    case '<': // T <- 1
                        return _cs.NextChar != '-';
                }

            }
            return true;
        }

        internal string GetIdentifier() {
            var start = _cs.Position;
            var identifier = string.Empty;

            SkipIdentifier();

            var length = _cs.Position - start;
            if (length >= 0) {
                identifier = _cs.Text.GetText(new TextRange(start, length));
            }

            return identifier;
        }

        /// <summary>
        /// Handle R comment. Comment starts with #
        /// and goes to the end of the line.
        /// </summary>
        private void HandleComment() {
            Tokenizer.HandleEolComment(_cs, (start, length) => {
                if (_separateComments) {
                    _comments.Add(new RToken(RTokenType.Comment, start, length));
                } else {
                    AddToken(RTokenType.Comment, start, length);
                }
            });
        }

        /// <summary>
        /// Adds a token that represent a string
        /// </summary>
        /// <param name="openQuote"></param>
        private void HandleString(char openQuote)
            => Tokenizer.HandleString(openQuote, _cs, (start, length) => AddToken(RTokenType.String, start, length));

        private bool AddIdentifier() {
            // 10.3.2 Identifiers
            // Identifiers consist of a sequence of letters, digits, the period (‘.’) and the underscore.
            // They must not start with a digit or an underscore, or with a period followed by a digit.
            // The definition of a letter depends on the current locale: the precise set of characters 
            // allowed is given by the C expression (isalnum(c) || c == ’.’ || c == ’_’) and will include
            // accented letters in many Western European locales.

            var start = _cs.Position;

            SkipIdentifier();

            if (_cs.Position > start) {
                AddToken(RTokenType.Identifier, start, _cs.Position - start);
                return true;
            }

            return false;
        }

        private void AddToken(RTokenType type, int start, int length) {
            var token = new RToken(type, start, length);
            _tokens.Add(token);
        }

        private void AddToken(RTokenType type, RTokenSubType subType, int start, int length) {
            var token = new RToken(type, start, length) { SubType = subType };
            _tokens.Add(token);
        }

        internal void SkipIdentifier() {
            // Handle backticks first. `anything` allow identifiers
            // to be of any syntax, not just standard names.
            if (_cs.CurrentChar == '`') {
                var closingBacktickIndex = _cs.IndexOf('`', _cs.Position + 1);
                if (closingBacktickIndex >= 0) {
                    _cs.Position = closingBacktickIndex + 1;
                } else {
                    _cs.Position = _cs.Length;
                }
            } else {
                Tokenizer.SkipIdentifier(
                    _cs,
                    (CharacterStream cs) => { return (_cs.IsLetter() || _cs.CurrentChar == '.'); },
                    (CharacterStream cs) => { return IsIdentifierCharacter(cs); });
            }
        }

        internal void SkipUnknown() {
            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace()) {
                if (_cs.CurrentChar == '#') {
                    break;
                }

                _cs.MoveToNextChar();
            }
        }

        private static bool IsIdentifierCharacter(CharacterStream cs) => IsIdentifierCharacter(cs.CurrentChar);

        public static bool IsIdentifierCharacter(char ch)
            => (CharacterStream.IsLetter(ch) || CharacterStream.IsDecimal(ch) || ch == '.' || ch == '_' || ch == '`');

        private static bool IsOpenBraceFollow(CharacterStream cs, int position) {
            for (var i = position; i < cs.Length; i++) {
                if (!char.IsWhiteSpace(cs[i])) {
                    return cs[i] == '(';
                }
            }

            return false;
        }
    }
}
