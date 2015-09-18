using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting
{
    /// <summary>
    /// Formats R code based on tokenization
    /// </summary>
    public class RFormatter
    {
        private RFormatOptions _options;
        private IndentBuilder _indentBuilder;
        private TextBuilder _tb;
        private TokenStream<RToken> _tokens;
        private ITextProvider _textProvider;
        private bool _suppressLineBreaks;

        public RFormatter() :
            this(new RFormatOptions())
        {
        }

        public RFormatter(RFormatOptions options)
        {
            _options = options;
            _indentBuilder = new IndentBuilder(_options.IndentType, _options.IndentSize, _options.TabSize);
            _tb = new TextBuilder(_indentBuilder);
        }

        /// <summary>
        /// Format string containing R code
        /// </summary>
        public string Format(string text)
        {
            // Tokenize incoming text
            Tokenize(text);

            // Add everything in the global scope
            AppendScope(stopAtCloseCurly: false, stopAtLineBreak: false);
            // Append any trailing line breaks
            AppendTextBeforeToken();

            return _tb.Text;
        }

        /// <summary>
        /// Iterates over tokens in the current scope and constructs formatted text
        /// </summary>
        /// <param name="stopAtLineBreak">
        /// If true scope stops at the nearest line break. Used when formatting
        /// simple conditional statements like 'if() stmt1 else stmt1' that
        /// should not be broken into multiple lines.
        /// </param>
        private void AppendScope(bool stopAtCloseCurly, bool stopAtLineBreak)
        {
            while (!_tokens.IsEndOfStream())
            {
                if (_tokens.CurrentToken.TokenType != RTokenType.OpenCurlyBrace)
                {
                    AppendTextBeforeToken();
                }

                AppendNextToken();

                if (stopAtLineBreak && _tokens.IsLineBreakAfter(_textProvider, _tokens.Position))
                {
                    break;
                }

                if (stopAtCloseCurly && _tokens.PreviousToken.TokenType == RTokenType.CloseCurlyBrace)
                {
                    break;
                }
            }
        }

        private void AppendNextToken()
        {
            switch (_tokens.CurrentToken.TokenType)
            {
                case RTokenType.Keyword:
                    AppendKeyword();
                    break;

                case RTokenType.OpenCurlyBrace:
                    AppendOpenCurly();
                    break;

                case RTokenType.CloseCurlyBrace:
                    AppendCloseCurly();
                    break;

                case RTokenType.Comma:
                    AppendComma();
                    break;

                case RTokenType.Semicolon:
                    AppendSemicolon();
                    break;

                case RTokenType.Operator:
                    AppendOperator();
                    break;

                default:
                    AppendConditionalSpace();
                    AppendToken();
                    break;
            }
        }

        private void AppendConditionalSpace()
        {
            switch (_tokens.PreviousToken.TokenType)
            {
                case RTokenType.OpenBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                case RTokenType.Operator:
                    break;

                default:
                    switch (_tokens.CurrentToken.TokenType)
                    {
                        case RTokenType.OpenBrace:
                        case RTokenType.OpenSquareBracket:
                        case RTokenType.OpenDoubleSquareBracket:
                        case RTokenType.CloseBrace:
                        case RTokenType.CloseSquareBracket:
                        case RTokenType.CloseDoubleSquareBracket:
                            break;

                        default:
                            if (!char.IsWhiteSpace(_tb.LastCharacter) && _tb.Length > 0)
                            {
                                _tb.AppendSpace();
                            }
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Appends keyword and its constructs such as condition that follows 'if' 
        /// And the following scope controlling indentation as appropriate.
        /// </summary>
        /// <returns>
        /// True if scope is closed and control flow should return 
        /// to the outer scope and the indent level should decrease.
        /// </returns>
        private void AppendKeyword()
        {
            if (!char.IsWhiteSpace(_tb.LastCharacter) && _tb.Length > 0)
            {
                _tb.AppendSpace();
            }

            string keyword = AppendToken();

            // Append space after the keyword as needed
            if (_options.SpaceAfterKeyword && !IsKeywordWithoutSpaces(keyword) &&
                (_tokens.CurrentToken.TokenType == RTokenType.OpenBrace || _tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace))
            {
                _tb.AppendSpace();
            }

            if (IsControlBlock(keyword))
            {
                // Keyword defines optional condition
                // and possibly new '{ }' scope
                AppendControlBlock(keyword);
            }
        }

        private void AppendControlBlock(string keyword)
        {
            if (IsConditionalBlock(keyword))
            {
                // Format condition such as (...) after 'if'
                AppendCondition();
            }

            // Format following scope
            AppendStatementsInBlock(keyword);
        }

        /// <summary>
        /// Formats conditional statement after 'if' or 'while'
        /// </summary>
        private void AppendCondition()
        {
            // Expected open brace
            if (_tokens.CurrentToken.TokenType != RTokenType.OpenBrace)
            {
                return;
            }

            var braceCounter = new BraceCounter<RToken>(new RToken(RTokenType.OpenBrace), new RToken(RTokenType.CloseBrace));
            braceCounter.CountBrace(_tokens.CurrentToken);
            AppendToken();

            while (braceCounter.Count > 0 && !_tokens.IsEndOfStream())
            {
                if (braceCounter.CountBrace(_tokens.CurrentToken))
                {
                    AppendToken();
                    continue;
                }

                switch (_tokens.CurrentToken.TokenType)
                {
                    case RTokenType.Keyword:
                        AppendKeyword();
                        break;

                    case RTokenType.Comma:
                        AppendComma();
                        break;

                    case RTokenType.Operator:
                        AppendOperator();
                        break;

                    default:
                        AppendConditionalSpace();
                        AppendToken();
                        break;
                }
            }
        }

        /// <summary>
        /// Appends statements inside scope that follows control block
        /// such as if() { } or a single statement that follows
        /// scope-less as in 'if() stmt' conditional.
        /// </summary>
        private void AppendStatementsInBlock(string keyword)
        {
            // May or may not have curly braces
            if (_tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace)
            {
                AppendScope(stopAtCloseCurly: true, stopAtLineBreak: false);
                return;
            }

            // No curly braces: single statement block
            bool foundSameLineElse = false;

            if (keyword == "if")
            {
                // Per R language spec:
                // <quote>
                //   The 'else' clause is optional. The statement if(any(x <= 0)) x <- x[x <= 0] 
                //   is valid. When the if statement is not in a block the else, if present, must 
                //   appear on the same line as the end of statement after if. Otherwise the new line 
                //   at the end of statement completes the if and yields a syntactically
                //   complete statement that is evaluated.
                // </quote>
                //
                // So we have to be very careful here. If 'if' is not scoped then we need
                // to check if 'else' is on the same line and then keep the line as is
                // i.e. format but not break into multiple lines. On the other hand,
                // if there is no 'else' then we can insert break after the 'if(...)'

                for (int i = _tokens.Position + 1; !foundSameLineElse && i < _tokens.Length; i++)
                {
                    if (_tokens[i].TokenType == RTokenType.OpenCurlyBrace || _tokens.IsLineBreakAfter(_textProvider, i))
                    {
                        break;
                    }

                    foundSameLineElse = _tokens[i].IsKeywordText(_textProvider, "else");
                }
            }

            if (foundSameLineElse)
            {
                _suppressLineBreaks = true;
                AppendScope(stopAtCloseCurly: false, stopAtLineBreak: true);
                _suppressLineBreaks = false;
            }
            else
            {
                if (_suppressLineBreaks)
                {
                    AppendScope(stopAtCloseCurly: true, stopAtLineBreak: true);
                }
                else
                {
                    bool addLineBreak = true;

                    // Special case: preserve like break between 'else' and 'if'
                    // if user put it there so 'else if' remains on one line
                    // if user didn't put line break between them.
                    if (keyword == "else" && _tokens.CurrentToken.IsKeywordText(_textProvider, "if") && !_tokens.IsLineBreakAfter(_textProvider, _tokens.Position - 1))
                    {
                        addLineBreak = false;
                    }

                    if (addLineBreak)
                    {
                        _tb.SoftLineBreak();
                        _tb.NewIndentLevel();

                        AppendScope(stopAtCloseCurly: true, stopAtLineBreak: true);
                        _tb.CloseIndentLevel();
                    }
                    else
                    {
                        _tb.AppendSpace();
                    }
                }
            }
        }

        private void AppendOpenCurly()
        {
            if (_suppressLineBreaks)
            {
                _tb.AppendSpace();
                AppendToken();
            }
            else
            {
                if (_options.BracesOnNewLine)
                {
                    _tb.SoftLineBreak();
                    AppendToken();
                    _tb.SoftLineBreak();
                    _tb.NewIndentLevel();
                }
                else
                {
                    _tb.AppendSpace();
                    AppendToken();

                    _tb.SoftLineBreak();
                    _tb.NewIndentLevel();
                }
            }
        }

        private void AppendCloseCurly()
        {
            if (_suppressLineBreaks)
            {
                _tb.AppendSpace();
            }
            else
            {
                _tb.SoftLineBreak();
                _tb.CloseIndentLevel();
                _tb.SoftIndent();
            }

            AppendToken();
        }

        private void AppendOperator()
        {
            if (IsOperatorWithoutSpaces())
            {
                AppendToken();
            }
            else
            {
                _tb.AppendSpace();
                AppendToken();
                _tb.AppendSpace();
            }
        }

        private void AppendComma()
        {
            AppendToken();
            if (_options.SpaceAfterComma)
            {
                _tb.AppendSpace();
            }
        }

        private void AppendSemicolon()
        {
            AppendToken();
            _tb.AppendSpace();
        }

        private string AppendToken()
        {
            string text = _textProvider.GetText(_tokens.CurrentToken);

            if (!char.IsWhiteSpace(_tb.LastCharacter))
            {
                if (_suppressLineBreaks)
                {
                    _tb.AppendSpace();
                }
                else if (_tb.LastCharacter == ')' && (char.IsLetter(text[0]) || text[0] == '_' || text[0] == '.'))
                {
                    _tb.AppendSpace();
                }
            }

            _tb.AppendText(text);
            _tokens.MoveToNextToken();

            return text;
        }

        private bool IsControlBlock(string text)
        {
            switch (text)
            {
                case "else":
                case "for":
                case "if":
                case "repeat":
                case "while":
                    return true;
            }

            return false;
        }

        private bool IsConditionalBlock(string text)
        {
            switch (text)
            {
                case "for":
                case "if":
                case "while":
                case "function":
                case "return":
                    return true;
            }

            return false;
        }

        private bool IsKeywordWithoutSpaces(string text)
        {
            switch (text)
            {
                case "library":
                case "typeof":
                    return true;
            }

            return false;
        }

        private bool IsOperatorWithoutSpaces()
        {
            string text = _textProvider.GetText(_tokens.CurrentToken);
            switch (text)
            {
                case "~":
                case "!":
                case ":":
                case "::":
                case ":::":
                case "$":
                case "@":
                    return true;
            }

            return false;
        }

        private void AppendTextBeforeToken()
        {
            int start = _tokens.Position > 0 ? _tokens.PreviousToken.End : 0;
            int end = _tokens.IsEndOfStream() ? _textProvider.Length : _tokens.CurrentToken.Start;

            string text = _textProvider.GetText(TextRange.FromBounds(start, end));
            if (string.IsNullOrWhiteSpace(text))
            {
                // Append any user-entered whitespace. We preserve 
                // line breaks but trim unnecessary spaces such as 
                // on empty lines. We must, however, preserve 
                // user indentation in long argument lists and
                // in expresions split into multiple lines.

                // We preserve user indentation of last token was 
                // open brace, square bracket, comma or an operator
                bool preserveUserIndent = false;
                if (_tokens.Position > 0)
                {
                    switch (_tokens.PreviousToken.TokenType)
                    {
                        case RTokenType.OpenBrace:
                        case RTokenType.OpenSquareBracket:
                        case RTokenType.OpenDoubleSquareBracket:
                        case RTokenType.Comma:
                        case RTokenType.Operator:
                            preserveUserIndent = true;
                            break;
                    }
                }

                _tb.CopyPrecedingLineBreaks(_textProvider, end);

                if (preserveUserIndent)
                {
                    int lastLineBreakIndex = text.LastIndexOfAny(new char[] { '\r', '\n' });
                    if (lastLineBreakIndex >= 0)
                    {
                        text = text.Substring(lastLineBreakIndex + 1);
                        int textIndentInSpaces = IndentBuilder.TextIndentInSpaces(text, _options.TabSize);
                        text = IndentBuilder.GetIndentString(textIndentInSpaces, _indentBuilder.IndentType, _indentBuilder.TabSize);
                        _tb.AppendPreformattedText(text);
                    }
                }
                else
                {
                    _tb.SoftIndent();
                }
            }
            else
            {
                // If there is unrecognized text between tokens, append it verbatim
                _tb.AppendPreformattedText(text);
            }
        }

        /// <summary>
        /// Tokenizes provided string that contains R code
        /// </summary>
        private void Tokenize(string text)
        {
            _textProvider = new TextStream(text);

            var tokenizer = new RTokenizer(separateComments: false);
            var tokens = tokenizer.Tokenize(_textProvider, 0, _textProvider.Length);
            _tokens = new TokenStream<RToken>(tokens, RToken.EndOfStreamToken);
        }
    }
}
