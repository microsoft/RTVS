using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Core.Tokens
{
    /// <summary>
    /// Main R tokenizer. Used for colirization and parsing. 
    /// Coloring of variables, function names and parameters
    /// is provided later by AST. Tokenizer only provides candidates.
    /// </summary>
    internal class Tokenizer : BaseTokenizer<RToken>
    {
        /// <summary>
        /// Main tokenization method. Responsible for adding next token
        /// to the list, if any. Returns if it is at the end of the 
        /// character stream. It is up to base class to terminate tokenization.
        /// </summary>
        public override void AddNextToken()
        {
            SkipWhitespace();

            if (_cs.IsEndOfStream())
                return;

            // First look at the numbers. Note that it is hard to tell
            // 12 +1 if it is a sum of numbers or a sequence. Note that
            // R also supports complex numbers like 1.e+01+-37.5i
            if (IsPossibleNumber())
            {
                int start = _cs.Position;

                int length = NumberTokenizer.HandleNumber(_cs);
                if (length > 0)
                {
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

        private void HandleCharacter()
        {
            switch (_cs.CurrentChar)
            {
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
                    if (_cs.NextChar == '[')
                    {
                        AddToken(RTokenType.OpenDoubleSquareBracket, _cs.Position, 2);
                        _cs.Advance(2);
                    }
                    else
                    {
                        AddToken(RTokenType.OpenSquareBracket, _cs.Position, 1);
                        _cs.MoveToNextChar();
                    }
                    break;

                case ']':
                    if (_cs.NextChar == ']')
                    {
                        AddToken(RTokenType.CloseDoubleSquareBracket, _cs.Position, 2);
                        _cs.Advance(2);
                    }
                    else
                    {
                        AddToken(RTokenType.CloseSquareBracket, _cs.Position, 1);
                        _cs.MoveToNextChar();
                    }
                    break;

                case '{':
                    AddToken(RTokenType.OpenCurlyBrace, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case '}':
                    AddToken(RTokenType.CloseCurlyBrace, _cs.Position, 1);
                    _cs.MoveToNextChar();
                    break;

                case '=':
                    AddToken(RTokenType.Operator, _cs.Position, 1);
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
                    if (_cs.CurrentChar == '.' && _cs.NextChar == '.' && _cs.LookAhead(2) == '.')
                    {
                        AddToken(RTokenType.Ellipsis, _cs.Position, 3);
                        _cs.Advance(3);
                    }
                    else
                    {
                        HandleOther();
                    }
                    break;
            }
        }

        internal bool IsPossibleNumber()
        {
            // It is hard to tell in 12 +1 if it is a sum of numbers or
            // a sequence. If operator or punctiation (comma, semicolon)
            // precedes the sign then sign is part of the number. 
            // Note that if preceding token is one of the function () 
            // or indexing braces [] then sign is an operator like in x[1]+2.
            // In other cases plus or minus is also a start of the operator. 
            // It important that in partial tokenization classifier removes
            // enough tokens so tokenizer can start its work early enough 
            // in the stream to be able to figure out numbers properly.

            if (_cs.CurrentChar == '-' || _cs.CurrentChar == '+')
            {
                // Next character must be decimal or a dot otherwise
                // it is not a number. No whitespace is allowed.
                if (CharacterStream.IsDecimal(_cs.NextChar) || _cs.NextChar == '.')
                {
                    // Check what previous token is, if any
                    if (_tokens.Count == 0)
                    {
                        // At the start of the file this can only be a number
                        return true;
                    }

                    RToken previousToken = _tokens[_tokens.Count - 1];

                    if (previousToken.TokenType == RTokenType.OpenBrace ||
                        previousToken.TokenType == RTokenType.OpenSquareBracket ||
                        previousToken.TokenType == RTokenType.Comma ||
                        previousToken.TokenType == RTokenType.Semicolon ||
                        previousToken.TokenType == RTokenType.Operator)
                    {
                        return true;
                    }
                }

                return false;
            }

            // R only supports 0xABCD. x0A is not legal.
            if (_cs.CurrentChar == '0' && _cs.NextChar == 'x')
            {
                // Hex humber like 0xA1BC
                return true;
            }

            if (_cs.IsDecimal())
            {
                return true;
            }

            if (_cs.CurrentChar == '.' && CharacterStream.IsDecimal(_cs.NextChar))
            {
                return true;
            }

            return false;
        }
        private void HandleNumber(int numberStart, int length)
        {
            if (_cs.CurrentChar == 'i')
            {
                _cs.MoveToNextChar();
                AddToken(RTokenType.Complex, numberStart, _cs.Position - numberStart);
                return;
            }

            // Check if this is actually complex number
            int imaginaryStart = _cs.Position;

            int imaginaryLength = NumberTokenizer.HandleImaginaryPart(_cs);
            if (imaginaryLength > 0)
            {
                AddToken(RTokenType.Complex, numberStart, length + imaginaryLength);
                return;
            }

            _cs.Position = imaginaryStart;
            AddToken(RTokenType.Number, numberStart, length);
        }

        private void HandleOther()
        {
            // Letter may be starting keyword, function or a variable name. 
            // At this point we should be either right after whitespace or 
            // at the beginning of the file.
            if (Char.IsLetter(_cs.CurrentChar))
            {
                // If this is not a keyword or a function name candidate
                HandleKeywordOrIdentifier();
                return;
            }

            // If character is not a letter and not start of a string it 
            // cannot be a keyword, function or variable name. Try operators 
            // first since they are longer than puctuation.
            if (HandleOperator())
            {
                return;
            }

            // Something unknown. Skip to whitespace and file as unknown.
            // Note however, we should take # ito account as it starts comment
            int start = _cs.Position;
            if (Char.IsLetter(_cs.CurrentChar))
            {
                AddIdentifier();
            }
            else
            {
                SkipUnknown();

                if (_cs.Position > start)
                {
                    AddToken(RTokenType.Unknown, start, _cs.Position - start);
                }
            }
        }

        /// <summary>
        /// Detemines if current position is at operator 
        /// and adds the appropriate token if so.
        /// </summary>
        /// <returns></returns>
        internal bool HandleOperator()
        {
            int length = Operators.OperatorLength(_cs);
            if (length > 0)
            {
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
        private void HandleKeywordOrIdentifier()
        {
            int start = _cs.Position;

            string s = this.GetIdentifier();
            if (s.Length > 0)
            {
                if (Keywords.IsKeyword(s))
                {
                    this.AddToken(RTokenType.Keyword, start, s.Length);
                }
                else if (Logicals.IsLogical(s))
                {
                    this.AddToken(RTokenType.Logical, start, s.Length);
                }
                else if (s == "NULL")
                {
                    this.AddToken(RTokenType.Null, RTokenSubType.BuiltinConstant, start, s.Length);
                }
                else if (s == "NA")
                {
                    this.AddToken(RTokenType.Missing, RTokenSubType.BuiltinConstant, start, s.Length);
                }
                else if (s == "Inf")
                {
                    this.AddToken(RTokenType.Infinity, RTokenSubType.BuiltinConstant, start, s.Length);
                }
                else if (s == "NaN")
                {
                    this.AddToken(RTokenType.NaN, RTokenSubType.BuiltinConstant, start, s.Length);
                }
                else
                {
                    RTokenSubType subType = RTokenSubType.None;

                    if(Builtins.IsBuiltin(s))
                    {
                        subType = RTokenSubType.BuiltinFunction;
                    }

                    this.AddToken(RTokenType.Identifier, subType, start, s.Length);
                }

                return;
            }

            _cs.MoveToNextChar();
        }

        internal string GetIdentifier()
        {
            int start = _cs.Position;
            string identifier = string.Empty;

            SkipIdentifier();

            int length = _cs.Position - start;
            if (length >= 0)
            {
                identifier = _cs.Text.GetText(new TextRange(start, length));
            }

            return identifier;
        }

        /// <summary>
        /// Handle R comment. Comment starts with #
        /// and goes to the end of the line.
        /// </summary>
        private void HandleComment()
        {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream() && !_cs.IsAtNewLine())
            {
                _cs.MoveToNextChar();
            }

            int length = _cs.Position - start;
            if (length > 0)
            {
                AddToken(RTokenType.Comment, start, length);
            }
        }

        /// <summary>
        /// Adds a token that represent a string
        /// </summary>
        /// <param name="openQuote"></param>
        private void HandleString(char openQuote)
        {
            int start = _cs.Position;

            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream())
            {
                if (_cs.CurrentChar == openQuote)
                {
                    _cs.MoveToNextChar();
                    break;
                }

                _cs.Advance(_cs.CurrentChar == '\\' ? 2 : 1);
            }

            int length = _cs.Position - start;
            if (length > 0)
            {
                AddToken(RTokenType.String, start, length);
            }
        }

        private void AddIdentifier()
        {
            // 10.3.2 Identifiers
            // Identifiers consist of a sequence of letters, digits, the period (‘.’) and the underscore.
            // They must not start with a digit or an underscore, or with a period followed by a digit.
            // The definition of a letter depends on the current locale: the precise set of characters 
            // allowed is given by the C expression (isalnum(c) || c == ’.’ || c == ’_’) and will include
            // accented letters in many Western European locales.

            int start = _cs.Position;

            SkipIdentifier();

            if (_cs.Position > start)
            {
                AddToken(RTokenType.Identifier, start, _cs.Position - start);
            }
        }

        private void AddToken(RTokenType type, int start, int length)
        {
            var token = new RToken(type, new TextRange(start, length));
            _tokens.Add(token);
        }

        private void AddToken(RTokenType type, RTokenSubType subType, int start, int length)
        {
            var token = new RToken(type, new TextRange(start, length));
            token.SubType = subType;
            _tokens.Add(token);
        }

        internal void SkipIdentifier()
        {
            if (!_cs.IsAnsiLetter())
                return;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace())
            {
                if (!IsIdentifierCharacter())
                    break;

                _cs.MoveToNextChar();
            }
        }

        internal void SkipUnknown()
        {
            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace())
            {
                if (_cs.CurrentChar == '#')
                    break;

                _cs.MoveToNextChar();
            }
        }

        private bool IsIdentifierCharacter()
        {
            return (_cs.IsAnsiLetter() || _cs.IsDecimal() || _cs.CurrentChar == '.' || _cs.CurrentChar == '_');
        }
    }
}
