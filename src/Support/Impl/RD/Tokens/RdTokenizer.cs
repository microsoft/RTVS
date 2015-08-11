using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Support.RD.Tokens
{
    /// <summary>
    /// Main R tokenizer. Used for colirization and parsing. 
    /// Coloring of variables, function names and parameters
    /// is provided later by AST. Tokenizer only provides candidates.
    /// </summary>
    internal class RdTokenizer : BaseTokenizer<RdToken>
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

            switch (_cs.CurrentChar)
            {
                case '\"':
                case '\'':
                    HandleString(_cs.CurrentChar);
                    break;

                case '%':
                    // RD Comments are from # to the end of the line
                    HandleComment();
                    break;

                case '#':
                    if (_cs.Position == 0 || _cs.PrevChar == '\r' || _cs.PrevChar == '\n')
                    {
                        HandlePragma();
                    }
                    else
                    {
                        _cs.MoveToNextChar();
                    }
                    break;

                case '\\':
                    if (IsEscape())
                    {
                        _cs.Advance(2);
                    }
                    else
                    {
                        HandleKeyword();
                    }
                    break;

                default:
                    _cs.MoveToNextChar();
                    break;
            }
        }

        private void HandleKeyword()
        {
            // Something unknown. Skip to whitespace and file as unknown.
            // Note however, we should take # ito account as it starts comment
            int start = _cs.Position;

            _cs.MoveToNextChar();
            SkipKeyword();

            if (_cs.Position - start > 1)
            {
                AddToken(RdTokenType.Keyword, start, _cs.Position - start);
            }

            SkipWhitespace();

            if (_cs.CurrentChar == '{' || _cs.CurrentChar == '[')
            {
                HandleArguments(_cs.CurrentChar == '{' ? '}' : ']');
            }
        }

        private void HandleArguments(char closingBrace)
        {
            AddToken(RdTokenType.OpenBrace, _cs.Position, 1);
            _cs.MoveToNextChar();

            int start = _cs.Position;

            while (!_cs.IsEndOfStream())
            {
                if (_cs.CurrentChar == '\\')
                {
                    if (IsEscape())
                    {
                        _cs.Advance(2);
                    }
                    else if(char.IsLetter(_cs.NextChar))
                    {
                        AddToken(RdTokenType.Argument, start, _cs.Position - start);

                        HandleKeyword();
                        start = _cs.Position;
                    }
                }

                if (_cs.CurrentChar == closingBrace)
                {
                    if (_cs.Position > start)
                    {
                        AddToken(RdTokenType.Argument, start, _cs.Position - start);
                    }

                    AddToken(RdTokenType.CloseBrace, _cs.Position, 1);

                    _cs.MoveToNextChar();
                    break;
                }

                _cs.MoveToNextChar();
            }

            if (_cs.CurrentChar == '{' || _cs.CurrentChar == '[')
            {
                HandleArguments(_cs.CurrentChar == '{' ? '}' : ']');
            }
        }

        private void HandlePragma()
        {
            int start = _cs.Position;
            SkipUnknown();

            int length = _cs.Position - start;
            if(length > 1)
            {
                AddToken(RdTokenType.Pragma, start, length);
            }
        }

        private bool IsEscape()
        {
            return _cs.NextChar == '%' || _cs.NextChar == '\\' || _cs.NextChar == '{' || _cs.NextChar == '}';
        }

        /// <summary>
        /// Handle RD comment. Comment starts with %
        /// and goes to the end of the line.
        /// </summary>
        private void HandleComment()
        {
            Tokenizer.HandleEolComment(_cs, (start, length) => AddToken(RdTokenType.Comment, start, length));
        }

        /// <summary>
        /// Adds a token that represent a string
        /// </summary>
        /// <param name="openQuote"></param>
        private void HandleString(char openQuote)
        {
            Tokenizer.HandleString(openQuote, _cs, (start, length) => AddToken(RdTokenType.String, start, length));
        }

        private void AddToken(RdTokenType type, int start, int length)
        {
            var token = new RdToken(type, new TextRange(start, length));
            _tokens.Add(token);
        }

        internal void SkipKeyword()
        {
            Tokenizer.SkipIdentifier(
                _cs,
                (CharacterStream cs) => { return _cs.IsAnsiLetter(); },
                (CharacterStream cs) => { return (_cs.IsAnsiLetter() || _cs.IsDecimal()); });
        }

        internal void SkipUnknown()
        {
            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace())
            {
                if (_cs.CurrentChar == '%')
                    break;

                _cs.MoveToNextChar();
            }
        }

        private bool IsIdentifierCharacter()
        {
            return (_cs.IsAnsiLetter() || _cs.IsDecimal());
        }
    }
}
