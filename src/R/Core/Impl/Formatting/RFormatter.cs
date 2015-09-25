using System.Linq;
using System.Collections.Generic;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;
using System.Diagnostics;

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

        private Stack<FormattingScope> _formattingScopes = new Stack<FormattingScope>();
        private Stack<RTokenType> _openBraces = new Stack<RTokenType>();

        private int SuppressLineBreakCount
        {
            get { return _formattingScopes.Peek().SuppressLineBreakCount; }
            set { _formattingScopes.Peek().SuppressLineBreakCount = value; }
        }

        public RFormatter() :
            this(new RFormatOptions())
        {
        }

        public RFormatter(RFormatOptions options)
        {
            _options = options;
            _indentBuilder = new IndentBuilder(_options.IndentType, _options.IndentSize, _options.TabSize);
            _tb = new TextBuilder(_indentBuilder);
            _formattingScopes.Push(new FormattingScope(_indentBuilder));
        }

        /// <summary>
        /// Format string containing R code
        /// </summary>
        public string Format(string text)
        {
            // Tokenize incoming text
            Tokenize(text);

            // Add everything in the global scope
            while (!_tokens.IsEndOfStream())
            {
                if (ShouldAppendTextBeforeToken())
                {
                    AppendTextBeforeToken();
                }

                AppendNextToken();
            }

            // Append any trailing line breaks
            AppendTextBeforeToken();

            return _tb.Text;
        }

        private void AppendNextToken()
        {
            switch (_tokens.CurrentToken.TokenType)
            {
                case RTokenType.Keyword:
                    AppendKeyword();
                    break;

                case RTokenType.OpenCurlyBrace:
                    OpenFormattingScope();
                    break;

                case RTokenType.CloseCurlyBrace:
                    CloseFormattingScope();
                    break;

                case RTokenType.Comma:
                    AppendToken(leadingSpace: false, trailingSpace: _options.SpaceAfterComma);
                    break;

                case RTokenType.Semicolon:
                    AppendToken(leadingSpace: false, trailingSpace: true);
                    _tb.SoftLineBreak();
                    break;

                case RTokenType.Operator:
                    AppendOperator();
                    break;

                default:
                    AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: false);
                    break;
            }
        }

        /// <summary>
        /// Opens new formatting scope. Scope is opened when either
        /// code discovers open curly brace or when line break
        /// suppression is on.
        /// </summary>
        private void OpenFormattingScope()
        {
            Debug.Assert(_tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace);

            if (IsInArguments())
            {
                FormattingScope formattingScope = new FormattingScope(_indentBuilder);
                if (formattingScope.Open(_textProvider, _tokens, _options))
                {
                    _formattingScopes.Push(formattingScope);
                }
            }

            if (_options.BracesOnNewLine)
            {
                _tb.SoftLineBreak();
            }
            else
            {
                if (!IsOpenBraceToken(_tokens.PreviousToken.TokenType))
                {
                    _tb.AppendSpace();
                }
            }

            AppendToken(leadingSpace: false, trailingSpace: false);

            _tb.SoftLineBreak();
            _tb.NewIndentLevel();
        }

        private void CloseFormattingScope()
        {
            Debug.Assert(_tokens.CurrentToken.TokenType == RTokenType.CloseCurlyBrace);

            _tb.SoftLineBreak();
            _tb.CloseIndentLevel();
            _tb.SoftIndent();

            if (_formattingScopes.Count > 1)
            {
                if (_formattingScopes.Peek().CloseBracePosition == _tokens.Position)
                {
                    FormattingScope scope = _formattingScopes.Pop();
                    scope.Close();
                }
            }

            AppendToken(leadingSpace: false, trailingSpace: false);

            if (SuppressLineBreakCount == 0 && !_tokens.IsEndOfStream())
            {
                // We insert line break after } unless next token is comma 
                // (scope is in the argument list) or a closing brace 
                // (last parameter in a function or indexer).
                if (!IsClosingToken(_tokens.CurrentToken.TokenType) && !IsInArguments())
                {
                    _tb.SoftLineBreak();
                }
            }
        }

        /// <summary>
        /// Iterates over tokens in the current scope and constructs formatted text
        /// </summary>
        /// <param name="stopAtLineBreak">
        /// If true scope stops at the nearest line break. Used when formatting
        /// simple conditional statements like 'if() stmt1 else stmt1' that
        /// should not be broken into multiple lines.
        /// </param>
        private void AppendScopeContent(bool stopAtLineBreak, bool stopAtElse = false)
        {
            while (!_tokens.IsEndOfStream())
            {
                if (ShouldAppendTextBeforeToken())
                {
                    AppendTextBeforeToken();
                }

                AppendNextToken();

                if (stopAtLineBreak && _tokens.IsLineBreakAfter(_textProvider, _tokens.Position))
                {
                    break;
                }

                if (_tokens.PreviousToken.TokenType == RTokenType.CloseCurlyBrace)
                {
                    break;
                }

                if (stopAtElse && _tokens.CurrentToken.IsKeywordText(_textProvider, "else"))
                {
                    break;
                }
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
            string keyword = _textProvider.GetText(_tokens.CurrentToken);
            bool controlBlock = IsControlBlock(keyword);

            // Append space after the keyword as needed
            bool trailingSpace = _options.SpaceAfterKeyword && !IsKeywordWithoutSpaces(keyword) &&
                    (_tokens.NextToken.TokenType == RTokenType.OpenBrace || _tokens.NextToken.TokenType == RTokenType.OpenCurlyBrace);

            AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: trailingSpace);

            if (controlBlock)
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
            AppendStatementsInScope(keyword);
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

            var braceCounter = new TokenBraceCounter<RToken>(new RToken(RTokenType.OpenBrace), new RToken(RTokenType.CloseBrace), new RTokenTypeComparer());
            braceCounter.CountBrace(_tokens.CurrentToken);

            AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: false);

            while (braceCounter.Count > 0 && !_tokens.IsEndOfStream())
            {
                if (braceCounter.CountBrace(_tokens.CurrentToken))
                {
                    AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: false);
                    continue;
                }

                switch (_tokens.CurrentToken.TokenType)
                {
                    case RTokenType.Keyword:
                        AppendKeyword();
                        break;

                    case RTokenType.Comma:
                        AppendToken(leadingSpace: false, trailingSpace: _options.SpaceAfterComma);
                        break;

                    case RTokenType.Operator:
                        AppendOperator();
                        break;

                    default:
                        AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: false);
                        break;
                }
            }
        }

        /// <summary>
        /// Appends statements inside scope that follows control block
        /// such as if() { } or a single statement that follows
        /// scope-less as in 'if() stmt' conditional.
        /// </summary>
        private void AppendStatementsInScope(string keyword)
        {
            // May or may not have curly braces
            if (_tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace)
            {
                // Regular { } scope so just handle it normally
                AppendScopeContent(stopAtLineBreak: false);

                if (keyword == "if" && _tokens.CurrentToken.IsKeywordText(_textProvider, "else"))
                {
                    // if (FALSE) {
                    //   x <- 1
                    // }
                    // else
                    AppendKeyword();
                }

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

                foundSameLineElse = HasSameLineElse();
                if (foundSameLineElse)
                {
                    SuppressLineBreakCount++;
                    AppendScopeContent(stopAtLineBreak: true);
                    SuppressLineBreakCount--;

                    return;
                }
            }

            if (SuppressLineBreakCount > 0)
            {
                AppendScopeContent(stopAtLineBreak: true);
                return;
            }

            bool addLineBreak = true;

            // Special case: preserve like break between 'else' and 'if'
            // if user put it there so 'else if' remains on one line
            // if user didn't put line break between them.

            if (IsInArguments() ||
                (keyword == "else" && _tokens.CurrentToken.IsKeywordText(_textProvider, "if") &&
                !_tokens.IsLineBreakAfter(_textProvider, _tokens.Position - 1)))
            {
                addLineBreak = false;
            }

            if (addLineBreak)
            {
                _tb.SoftLineBreak();
                _tb.NewIndentLevel();

                // This scope-less 'if' so we have to stop at the end 
                // of the line as in
                //  if (TRUE)
                //      x <- 1
                //  else {
                //  }

                AppendScopeContent(stopAtLineBreak: true, stopAtElse: true);

                //  if (TRUE)
                //      repeat {
                //      }
                //  else
                //
                _tb.CloseIndentLevel();
            }
            else
            {
                _tb.AppendSpace();
            }
        }

        private static bool IsOpenBraceToken(RTokenType tokenType)
        {
            switch (tokenType)
            {
                case RTokenType.OpenBrace:
                case RTokenType.OpenCurlyBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                    return true;
            }

            return false;
        }

        private static bool IsClosingToken(RTokenType tokenType)
        {
            switch (tokenType)
            {
                case RTokenType.Comma:
                case RTokenType.CloseBrace:
                case RTokenType.CloseSquareBracket:
                case RTokenType.CloseDoubleSquareBracket:
                case RTokenType.Semicolon:
                    return true;
            }

            return false;
        }

        private void AppendOperator()
        {
            if (IsOperatorWithoutSpaces())
            {
                AppendToken(leadingSpace: false, trailingSpace: false);
            }
            else
            {
                AppendToken(leadingSpace: true, trailingSpace: true);
            }
        }

        private string AppendToken(bool leadingSpace, bool trailingSpace)
        {
            if (leadingSpace)
            {
                _tb.AppendSpace();
            }

            string text = _textProvider.GetText(_tokens.CurrentToken);
            _tb.AppendText(text);

            HandleBrace();
            _tokens.MoveToNextToken();

            if (trailingSpace)
            {
                _tb.AppendSpace();
            }

            return text;
        }

        private void HandleBrace()
        {
            switch (_tokens.CurrentToken.TokenType)
            {
                case RTokenType.OpenBrace:
                case RTokenType.OpenCurlyBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                    _openBraces.Push(_tokens.CurrentToken.TokenType);
                    return;
            }

            if (_openBraces.Count > 0)
            {
                switch (_tokens.CurrentToken.TokenType)
                {
                    case RTokenType.CloseBrace:
                    case RTokenType.CloseSquareBracket:
                    case RTokenType.CloseDoubleSquareBracket:
                        if (_openBraces.Peek() == GetMatchingBraceToken(_tokens.CurrentToken.TokenType))
                        {
                            _openBraces.Pop();
                        }
                        break;

                    case RTokenType.CloseCurlyBrace:
                        // Close all braces until the nearest curly
                        while (_openBraces.Peek() != RTokenType.OpenCurlyBrace && _openBraces.Count > 0)
                        {
                            _openBraces.Pop();
                        }

                        if (_openBraces.Count > 0)
                        {
                            _openBraces.Pop();
                        }
                        break;
                }
            }
        }

        private RTokenType GetMatchingBraceToken(RTokenType tokenType)
        {
            switch (tokenType)
            {
                case RTokenType.OpenBrace:
                    return RTokenType.CloseBrace;

                case RTokenType.CloseBrace:
                    return RTokenType.OpenBrace;

                case RTokenType.OpenSquareBracket:
                    return RTokenType.CloseSquareBracket;

                case RTokenType.CloseSquareBracket:
                    return RTokenType.OpenSquareBracket;

                case RTokenType.OpenDoubleSquareBracket:
                    return RTokenType.CloseDoubleSquareBracket;

                case RTokenType.CloseDoubleSquareBracket:
                    return RTokenType.OpenDoubleSquareBracket;
            }

            Debug.Assert(false, "Unknown brace token");
            return RTokenType.Unknown;
        }

        private bool LeadingSpaceNeeded()
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
                                return true;
                            }
                            break;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Tells if scope is opening inside function
        /// or indexer arguments and hence user indentation
        /// of curly braces must be respected.
        /// </summary>
        /// <returns></returns>
        private bool IsInArguments()
        {
            if (_openBraces.Count > 0)
            {
                switch (_openBraces.Peek())
                {
                    case RTokenType.OpenBrace:
                    case RTokenType.OpenSquareBracket:
                    case RTokenType.OpenDoubleSquareBracket:
                        return true;
                }
            }

            return false;
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
                case "function":
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

        private bool ShouldAppendTextBeforeToken()
        {
            return !IsClosingToken(_tokens.CurrentToken.TokenType) && _tokens.CurrentToken.TokenType != RTokenType.OpenCurlyBrace;
        }

        private bool HasSameLineElse()
        {
            bool foundSameLineElse = false;

            for (int i = _tokens.Position; !foundSameLineElse && i < _tokens.Length; i++)
            {
                if (_tokens[i].TokenType == RTokenType.OpenCurlyBrace || _tokens.IsLineBreakAfter(_textProvider, i))
                {
                    break;
                }

                if (_tokens[i].TokenType == RTokenType.Keyword)
                {
                    foundSameLineElse = _tokens[i].IsKeywordText(_textProvider, "else");
                    break; // any keyword breaks if/else sequence as in if(TRUE) if(FALSE) ...
                }
            }

            return foundSameLineElse;
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
