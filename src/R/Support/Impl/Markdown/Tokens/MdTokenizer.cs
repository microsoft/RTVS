using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Support.Markdown.Tokens
{
    /// <summary>
    /// Main regular markdown tokenizer. R Markdown has 
    /// a separate tokenizer.
    /// https://help.github.com/articles/markdown-basics/
    /// </summary>
    internal class MdTokenizer : BaseTokenizer<MdToken>
    {
        public MdTokenizer()
        {
        }

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

            HandleCharacter();
        }

        protected virtual void HandleCharacter()
        {
            while (!_cs.IsEndOfStream())
            {
                bool handled = false;

                // Regular content is Latex-like
                switch (_cs.CurrentChar)
                {
                    case '#':
                        handled = HandleHeading();
                        break;

                    case '*':
                        handled = HandleStar();
                        break;

                    case '_':
                        if (!char.IsWhiteSpace(_cs.NextChar))
                        {
                            handled = HandleItalic('_', MdTokenType.Italic);
                        }
                        break;

                    case '>':
                        handled = HandleQuote();
                        break;

                    case '`':
                        handled = HandleBackTick();
                        break;

                    case '-':
                        if (_cs.NextChar == ' ')
                        {
                            handled = HandleListItem();
                        }
                        else if (_cs.NextChar == '-' && _cs.LookAhead(2) == '-')
                        {
                            handled = HandleHeading();
                        }
                        break;

                    case '=':
                        if (_cs.NextChar == '=' && _cs.LookAhead(2) == '=')
                        {
                            handled = HandleHeading();
                        }
                        break;

                    case '[':
                        handled = HandleAltText();
                        break;

                    default:
                        if (_cs.IsDecimal())
                        {
                            handled = HandleNumberedListItem();
                        }
                        break;

                }

                if (!handled)
                {
                    _cs.MoveToNextChar();
                }
            }
        }

        private bool HandleHeading()
        {
            if (_cs.Position == 0 || _cs.PrevChar == '\n' || _cs.PrevChar == '\r')
            {
                return HandleSequenceToEol(MdTokenType.Heading);
            }

            return false;
        }

        private bool HandleQuote()
        {
            if (_cs.Position == 0 || _cs.PrevChar == '\n' || _cs.PrevChar == '\r')
            {
                if (_cs.NextChar == ' ')
                    return HandleSequenceToEmptyLine(MdTokenType.Blockquote);
            }

            return false;
        }

        private bool HandleAltText()
        {
            int start = _cs.Position;
            while (!_cs.IsEndOfStream())
            {
                if (_cs.CurrentChar == ']' || _cs.IsAtNewLine())
                    break;

                _cs.MoveToNextChar();
            }

            if (_cs.CurrentChar == ']' && _cs.NextChar == '(')
            {
                int end = _cs.Position + 1;
                _cs.Advance(2);

                while (!_cs.IsEndOfStream())
                {
                    if (_cs.CurrentChar == ')' || _cs.IsAtNewLine())
                        break;

                    _cs.MoveToNextChar();
                }

                if (_cs.CurrentChar == ')')
                {
                    AddToken(MdTokenType.AltText, start, end - start);
                }
            }

            return true;
        }

        private bool HandleBackTick()
        {
            if (_cs.NextChar == '`' && _cs.LookAhead(2) == '`' && (_cs.Position == 0 || _cs.PrevChar == '\n' || _cs.PrevChar == '\r'))
            {
                return HandleCode();
            }

            return HandleMonospace();
        }

        private bool HandleCode()
        {
            int start = _cs.Position;
            _cs.Advance(3);

            while (!_cs.IsEndOfStream())
            {
                if (_cs.IsAtNewLine() && _cs.NextChar == '`' && _cs.LookAhead(2) == '`' && _cs.LookAhead(3) == '`')
                {
                    _cs.Advance(4);
                    AddToken(MdTokenType.Code, start, _cs.Position - start);
                    return true;
                }

                _cs.MoveToNextChar();
            }

            return false;
        }

        private bool HandleMonospace()
        {
            int start = _cs.Position;
            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream())
            {
                if (_cs.CurrentChar == '`')
                {
                    _cs.MoveToNextChar();
                    AddToken(MdTokenType.Monospace, start, _cs.Position - start);
                    return true;
                }

                _cs.MoveToNextChar();
            }

            return false;
        }

        private bool HandleStar()
        {
            int start = _cs.Position;

            switch (_cs.NextChar)
            {
                case '*':
                    if (!char.IsWhiteSpace(_cs.LookAhead(2)))
                    {
                        return HandleBold(MdTokenType.Bold);
                    }
                    break;

                case ' ':
                    return HandleListItem();

                default:
                    if (!char.IsWhiteSpace(_cs.NextChar))
                    {
                        return HandleItalic('*', MdTokenType.Italic);
                    }
                    break;
            }

            return false;
        }

        private bool HandleBold(MdTokenType tokenType)
        {
            int start = _cs.Position;

            _cs.Advance(2);
            while (!_cs.IsEndOfStream())
            {
                if (_cs.CurrentChar == '_' || (_cs.CurrentChar == '*' && _cs.NextChar != '*'))
                {
                    int tokenCount = _tokens.Count;
                    AddToken(tokenType, start, _cs.Position - start);

                    int startOfItalic = _cs.Position;
                    if (HandleItalic(_cs.CurrentChar, MdTokenType.BoldItalic))
                    {
                        start = _cs.Position;
                    }
                    else
                    {
                        _tokens.RemoveRange(tokenCount, _tokens.Count - tokenCount);
                        _cs.Position = startOfItalic;
                        break;
                    }
                }

                if (_cs.CurrentChar == '*' && _cs.NextChar == '*')
                {
                    _cs.Advance(2);
                    AddToken(tokenType, start, _cs.Position - start);
                    return true;
                }

                if (_cs.IsAtNewLine())
                    break;

                _cs.MoveToNextChar();
            }

            return false;
        }

        private bool HandleItalic(char boundaryChar, MdTokenType tokenType)
        {
            int start = _cs.Position;

            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream())
            {
                if (_cs.CurrentChar == '*' && _cs.NextChar == '*')
                {
                    int tokenCount = _tokens.Count;
                    AddToken(tokenType, start, _cs.Position - start);

                    int startOfBold = _cs.Position;
                    if (HandleBold(MdTokenType.BoldItalic))
                    {
                        start = _cs.Position;
                    }
                    else
                    {
                        _tokens.RemoveRange(tokenCount, _tokens.Count - tokenCount);
                        _cs.Position = startOfBold;
                        break;
                    }
                }

                if (_cs.CurrentChar == boundaryChar)
                {
                    _cs.MoveToNextChar();
                    AddToken(tokenType, start, _cs.Position - start);
                    return true;
                }

                if (_cs.IsAtNewLine())
                    break;

                _cs.MoveToNextChar();
            }

            return false;
        }

        private bool HandleListItem()
        {
            // List item must start at the beginning of the line
            bool atStartOfLine = _cs.Position == 0;

            if (!atStartOfLine)
            {
                for (int i = _cs.Position - 1; i >= 0; i--)
                {
                    char ch = _cs[i];

                    if (!char.IsWhiteSpace(ch))
                    {
                        break;
                    }

                    if (ch == '\r' || ch == '\n')
                    {
                        atStartOfLine = true;
                        break;
                    }
                }
            }

            if (atStartOfLine)
            {
                return HandleSequenceToEol(MdTokenType.ListItem);
            }

            return false;
        }

        private bool HandleNumberedListItem()
        {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream())
            {
                if (!_cs.IsDecimal())
                {
                    if (_cs.CurrentChar == '.' && char.IsWhiteSpace(_cs.NextChar))
                    {
                        return HandleSequenceToEol(MdTokenType.ListItem, start);
                    }

                    break;
                }

                _cs.MoveToNextChar();
            }

            return false;
        }

        private bool HandleSequenceToEol(MdTokenType tokeType, int startPosition = -1)
        {
            int start = startPosition >= 0 ? startPosition : _cs.Position;
            _cs.SkipToEol();

            AddToken(tokeType, start, _cs.Position - start);
            return true;
        }

        private bool HandleSequenceToEmptyLine(MdTokenType tokeType)
        {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream())
            {
                _cs.SkipToEol();
                _cs.SkipLineBreak();

                if (_cs.IsAtNewLine())
                {
                    break;
                }
            }

            AddToken(tokeType, start, _cs.Position - start);
            return true;
        }

        private void AddToken(MdTokenType type, int start, int length)
        {
            if (length > 0)
            {
                var token = new MdToken(type, new TextRange(start, length));
                _tokens.Add(token);
            }
        }

        /// <summary>
        /// Skips content until the nearest whitespace
        /// </summary>
        internal void SkipToWhitespace()
        {
            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace())
            {
                _cs.MoveToNextChar();
            }
        }
    }
}
