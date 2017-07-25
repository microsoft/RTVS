// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Formats R code based on tokenization
    /// </summary>
    public class RFormatter {
        private readonly RFormatOptions _options;
        private readonly IndentBuilder _indentBuilder;
        private readonly TextBuilder _tb;
        private readonly FormattingScopeStack _formattingScopes = new FormattingScopeStack();
        private TokenStream<RToken> _tokens;
        private ITextProvider _textProvider;
        private int _singleLineScopeEnd = -1;
        private BraceHandler _braceHandler;
        private ExpressionHelper _expressionHelper;

        public RFormatter() :
            this(new RFormatOptions()) {
        }

        public RFormatter(RFormatOptions options) {
            _options = options;
            _indentBuilder = new IndentBuilder(_options.IndentType, _options.IndentSize, _options.TabSize);
            _tb = new TextBuilder(_indentBuilder);
        }

        /// <summary>
        /// Format string containing R code
        /// </summary>
        public string Format(string text) {
            _tb.LineBreak = text.GetDefaultLineBreakSequence();

            // Tokenize incoming text
            Tokenize(text);

            // Add everything in the global scope
            while (!_tokens.IsEndOfStream()) {
                if (ShouldAppendTextBeforeToken()) {
                    AppendTextBeforeToken();
                }

                AppendNextToken();
            }
            // Append any trailing line breaks
            AppendTextBeforeToken();

            return _tb.Text;
        }

        private void AppendNextToken() {
            switch (_tokens.CurrentToken.TokenType) {
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
                    AppendComma();
                    break;

                case RTokenType.Semicolon:
                    AppendToken(leadingSpace: false, trailingSpace: true);
                    if (_tokens.IsLineBreakAfter(_textProvider, _tokens.Position) || _options.BreakMultipleStatements) {
                        SoftLineBreak();
                    }
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
        private void OpenFormattingScope() {
            Debug.Assert(_tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace);

            // If scope is empty, make it { } unless there is a line break already in it
            if (_tokens.NextToken.TokenType == RTokenType.CloseCurlyBrace &&
                _textProvider.IsWhiteSpaceOnlyRange(_tokens.CurrentToken.End, _tokens.NextToken.Start)) {
                var ls = _tokens.PreviousToken.TokenType == RTokenType.CloseBrace && _options.SpaceBeforeCurly;
                AppendToken(leadingSpace: ls, trailingSpace: true);
                AppendToken(leadingSpace: false, trailingSpace: false);
                return;
            }

            // Determine if the scope is a single line like if(TRUE) { 1 } and keep it 
            // on a single line unless there are line breaks in it. Continue without 
            // breaks until the scope ends. Includes multiple nested scopes such as {{{ 1 }}}. 
            // We continue on the outer scope boundaries.
            if (_singleLineScopeEnd < 0) {
                _singleLineScopeEnd = GetSingleLineScopeEnd();
            }

            if (_options.BracesOnNewLine && !SingleLineScope) {
                // Add line break if brace positioning is expanded and it is not a single line scope like { }
                SoftLineBreak();
            } else {
                // Add space if curly is preceded by )
                bool precededByCloseBrace = _tokens.PreviousToken.TokenType == RTokenType.CloseBrace;
                if (precededByCloseBrace && _options.SpaceBeforeCurly) {
                    _tb.AppendSpace();
                }
            }

            // Append { to the formatted text
            AppendToken(leadingSpace: false, trailingSpace: false);

            // Open new scope. It is one indent level deeper than either current
            // indent level or one level deeper than the current line indent
            // such as when formatting multiline expression that also includes
            // scope statements such as
            //   x <- 
            //      if(...) {
            //          z
            //      }
            //
            _formattingScopes.OpenScope(new FormattingScope(_tb, _tokens, _tokens.Position - 1, _options, _braceHandler));

            if (!SingleLineScope) {
                _tb.SoftLineBreak();
            }
        }

        private void CloseFormattingScope() {
            Debug.Assert(_tokens.CurrentToken.TokenType == RTokenType.CloseCurlyBrace);

            // if it is not single line scope like { } add linke break before }
            if (!SingleLineScope) {
                _tb.SoftLineBreak();
            }

            var leadingSpace = SingleLineScope && _tokens.PreviousToken.TokenType != RTokenType.CloseCurlyBrace;

            // Close formatting scope and remember if it was based on the user-supplied indent.
            _formattingScopes.TryCloseScope(_tokens.Position);

            // Closing curly indentation is defined by the line that either holds the opening curly brace
            // or the line that holds keyword that defines the expression that the curly belongs to.
            // Examples: 
            //
            //      x <- 
            //          function(a) {
            //          }
            //
            //      x <- function(a) {
            //      }
            //
            if (!SingleLineScope) {
                int lineIndentSize = _braceHandler.GetCloseCurlyBraceIndentSize(_tokens.CurrentToken, _tb, _options);
                if (lineIndentSize > 0) {
                    var indentString = IndentBuilder.GetIndentString(lineIndentSize, _options.IndentType, _options.TabSize);
                    _tb.AppendPreformattedText(indentString);
                    leadingSpace = false;
                } else {
                    _tb.SoftIndent();
                }
            }

            AppendToken(leadingSpace: leadingSpace, trailingSpace: false);

            bool singleLineScopeJustClosed = false;
            if (_tokens.CurrentToken.Start >= _singleLineScopeEnd) {
                _singleLineScopeEnd = -1;
                singleLineScopeJustClosed = true;
            }

            if (_formattingScopes.SuppressLineBreakCount == 0 && !_tokens.IsEndOfStream()) {
                // We insert line break after } unless 
                //  a) Next token is comma (scope is in the argument list) or 
                //  b) Next token is a closing brace (last parameter in a function or indexer) or 
                //  c) Next token is by 'else' (so 'else' does not get separated from 'if') or
                //  d) We are in a single-line scope sequence like if() {{ }}
                if (!KeepCurlyAndElseTogether()) {
                    if (singleLineScopeJustClosed &&
                        !IsClosingToken(_tokens.CurrentToken) &&
                        !_braceHandler.IsInArguments()) {
                        SoftLineBreak();
                    }
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
        private void AppendScopeContent(bool stopAtLineBreak, bool stopAtElse = false) {
            while (!_tokens.IsEndOfStream()) {
                if (ShouldAppendTextBeforeToken()) {
                    AppendTextBeforeToken();

                    // If scope is simple (no curly braces) then stopAtLineBreak is true.
                    // If there is a line break before start of the simple scope content
                    // as in 'if(true)\nx<-1' we need to add indent
                    if (stopAtLineBreak && _tb.IsAtNewLine) {
                        _tb.AppendPreformattedText(IndentBuilder.GetIndentString(_options.IndentSize, _options.IndentType, _options.TabSize));
                    }
                }

                AppendNextToken();

                if (stopAtLineBreak && _tokens.IsLineBreakAfter(_textProvider, _tokens.Position)) {
                    break;
                }
                if (_tokens.PreviousToken.TokenType == RTokenType.CloseCurlyBrace) {
                    break;
                }
                if (stopAtElse && _tokens.CurrentToken.IsKeywordText(_textProvider, "else")) {
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
        private void AppendKeyword() {
            string keyword = _textProvider.GetText(_tokens.CurrentToken);
            bool controlBlock = IsControlBlock(keyword);

            // Append space after the keyword as needed
            bool trailingSpace = _options.SpaceAfterKeyword && !IsKeywordWithoutSpaces(keyword) &&
                    (_tokens.NextToken.TokenType == RTokenType.OpenBrace || _tokens.NextToken.TokenType == RTokenType.OpenCurlyBrace);

            AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: trailingSpace);

            if (controlBlock) {
                // Keyword defines optional condition and possibly new '{ }' scope
                AppendControlBlock(keyword);
            }
        }

        private void AppendControlBlock(string keyword) {
            if (IsConditionalBlock(keyword)) {
                // Format condition such as (...) after 'if'
                AppendCondition();
            }
            // Format following scope
            AppendStatementsInScope(keyword);
        }

        /// <summary>
        /// Formats conditional statement after 'if' or 'while'
        /// </summary>
        private void AppendCondition() {
            // Expected open brace
            if (_tokens.CurrentToken.TokenType != RTokenType.OpenBrace) {
                return;
            }

            var braceCounter = new TokenBraceCounter<RToken>(new RToken(RTokenType.OpenBrace), new RToken(RTokenType.CloseBrace), new RTokenTypeComparer());
            braceCounter.CountBrace(_tokens.CurrentToken);

            AppendToken(leadingSpace: LeadingSpaceNeeded(), trailingSpace: false);

            while (braceCounter.Count > 0 && !_tokens.IsEndOfStream()) {
                braceCounter.CountBrace(_tokens.CurrentToken);

                if (ShouldAppendTextBeforeToken()) {
                    AppendTextBeforeToken();
                }

                switch (_tokens.CurrentToken.TokenType) {
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
        private void AppendStatementsInScope(string keyword) {
            // May or may not have curly braces
            if (_tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace) {
                // Regular { } scope so just handle it normally
                AppendScopeContent(stopAtLineBreak: false);

                if (keyword == "if" &&
                    _tokens.CurrentToken.IsKeywordText(_textProvider, "else") &&
                    !_tokens.IsLineBreakAfter(_textProvider, _tokens.Position - 1)) {
                    // if (FALSE) {
                    //   x <- 1
                    // } else
                    // i.e. keep 'else' at the same line except when user did add line break as in
                    // if (...) { 1 }
                    // else { 2 }

                    if (!_options.BracesOnNewLine && _tokens.PreviousToken.TokenType == RTokenType.CloseCurlyBrace) {
                        while (_tb.LastCharacter.IsLineBreak()) {
                            // Undo line break
                            _tb.Remove(_tb.Length - 1, 1);
                        }

                        _tb.AppendPreformattedText(" ");
                    }
                    AppendKeyword();
                }
                return;
            }

            // No curly braces: single statement block
            bool foundSameLineElse = false;

            if (keyword == "if") {
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
                if (foundSameLineElse) {
                    _formattingScopes.SuppressLineBreakCount++;
                    AppendScopeContent(stopAtLineBreak: true);
                    _formattingScopes.SuppressLineBreakCount--;

                    return;
                }
            }

            if (_formattingScopes.SuppressLineBreakCount > 0) {
                AppendScopeContent(stopAtLineBreak: true);
                return;
            }

            bool addLineBreak = true;

            // Special case: preserve like break between 'else' and 'if'
            // if user put it there  'else if' remains on one line
            // if user didn't then add line break between them.

            if (_braceHandler.IsInArguments()) {
                addLineBreak = false;
            } else if (!_tokens.IsLineBreakAfter(_textProvider, _tokens.Position - 1)) {
                if (keyword.EqualsOrdinal("else") && _tokens.CurrentToken.IsKeywordText(_textProvider, "if")) {
                    addLineBreak = false;
                } else if ((keyword.EqualsOrdinal("if") || keyword.EqualsOrdinal("else") || keyword.EqualsOrdinal("repeat")) &&
                           _tokens.CurrentToken.TokenType != RTokenType.OpenCurlyBrace) {
                    // Preserve single-line conditionals like 'if (true) x <- 1'
                    addLineBreak = false;
                }
            }

            if (addLineBreak) {
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
            } else {
                _tb.AppendSpace();
            }
        }

        private static bool IsOpenBraceToken(RTokenType tokenType) {
            switch (tokenType) {
                case RTokenType.OpenBrace:
                case RTokenType.OpenCurlyBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                    return true;
            }

            return false;
        }

        private bool IsClosingToken(RToken token, AstRoot ast = null) {
            switch (token.TokenType) {
                case RTokenType.Comma:
                case RTokenType.CloseBrace:
                case RTokenType.CloseSquareBracket:
                case RTokenType.CloseDoubleSquareBracket:
                case RTokenType.Semicolon:
                    return true;
            }

            return false;
        }

        private bool KeepCurlyAndElseTogether() {
            if (_tokens.CurrentToken.TokenType != RTokenType.Keyword) {
                return false;
            }

            return _tokens.PreviousToken.TokenType == RTokenType.CloseCurlyBrace &&
                   _textProvider.GetText(_tokens.CurrentToken) == "else";
        }

        private void AppendOperator() {
            string text = _textProvider.GetText(_tokens.CurrentToken);
            if (_tokens.Position == 0 || TokenOperator.IsUnaryOperator(_tokens, _textProvider, TokenOperator.GetOperatorType(text), -1)) {
                AppendToken(leadingSpace: false, trailingSpace: false);
            } else if (IsOperatorWithoutSpaces(text)) {
                AppendToken(leadingSpace: false, trailingSpace: false);
            } else {
                if (_tb.IsAtNewLine && _tb.Length > 0) {
                    AppendTextBeforeToken(preserveUserIndent: true);
                    AppendToken(leadingSpace: false, trailingSpace: true);
                } else {
                    AppendToken(leadingSpace: true, trailingSpace: true);
                }
            }
        }

        private void AppendToken(bool leadingSpace, bool trailingSpace) {
            if (leadingSpace) {
                _tb.AppendSpace();
            }

            if (_tokens.CurrentToken.TokenType == RTokenType.Comment &&
                _tokens.Position > 0 &&
                _tokens.IsLineBreakAfter(_textProvider, _tokens.Position - 1)) {
                _tb.SoftIndent();
            }

            string text = _textProvider.GetText(_tokens.CurrentToken);
            if (text.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                _tb.AppendPreformattedText(text);
            } else {
                _tb.AppendText(text);
            }

            if (_tokens.CurrentToken.TokenType == RTokenType.Comment) {
                // make sure there is a line break between comment
                // and the next token
                _tb.SoftLineBreak();
            } else {
                _braceHandler.HandleBrace();
            }

            _tokens.MoveToNextToken();

            if (trailingSpace) {
                _tb.AppendSpace();
            }
        }





        private bool LeadingSpaceNeeded() {
            switch (_tokens.PreviousToken.TokenType) {
                case RTokenType.OpenBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                case RTokenType.Operator:
                case RTokenType.Comma:
                    break;

                default:
                    switch (_tokens.CurrentToken.TokenType) {
                        case RTokenType.OpenBrace:
                        case RTokenType.OpenSquareBracket:
                        case RTokenType.OpenDoubleSquareBracket:
                        case RTokenType.CloseBrace:
                        case RTokenType.CloseSquareBracket:
                        case RTokenType.CloseDoubleSquareBracket:
                            break;

                        default:
                            if (!char.IsWhiteSpace(_tb.LastCharacter) && _tb.Length > 0) {
                                return true;
                            }
                            break;
                    }
                    break;
            }

            return false;
        }

        private bool IsControlBlock(string text) {
            switch (text) {
                case "else":
                case "for":
                case "if":
                case "repeat":
                case "while":
                    return true;
            }

            return false;
        }

        private bool IsConditionalBlock(string text) {
            switch (text) {
                case "for":
                case "if":
                case "while":
                case "function":
                    return true;
            }

            return false;
        }

        private bool IsKeywordWithoutSpaces(string text) {
            switch (text) {
                case "function":
                    return true;
            }

            return false;
        }

        private bool IsOperatorWithoutSpaces(string text) {
            switch (text) {
                case "!":
                case ":":
                case "::":
                case ":::":
                case "$":
                case "@":
                    return true;
                case "=":
                    return !_options.SpacesAroundEquals;
            }

            return false;
        }

        private void AppendTextBeforeToken(bool preserveUserIndent = false) {
            int start = _tokens.Position > 0 ? _tokens.PreviousToken.End : 0;
            int end = _tokens.IsEndOfStream() ? _textProvider.Length : _tokens.CurrentToken.Start;

            string text = _textProvider.GetText(TextRange.FromBounds(start, end));
            bool whitespaceOnly = string.IsNullOrWhiteSpace(text);

            if (!preserveUserIndent && whitespaceOnly) {
                // Append any user-entered whitespace. We preserve line breaks but trim 
                // unnecessary spaces such as on empty lines. We must, however, preserve 
                // user indentation in long argument lists and in expresions split 
                // into multiple lines.

                // We preserve user indentation of last token was 
                // open brace, square bracket, comma or an operator
                if (_tokens.Position > 0) {
                    switch (_tokens.PreviousToken.TokenType) {
                        case RTokenType.OpenBrace:
                        case RTokenType.OpenSquareBracket:
                        case RTokenType.OpenDoubleSquareBracket:
                        case RTokenType.Comma:
                        case RTokenType.Operator:
                            preserveUserIndent = true;
                            break;

                        case RTokenType.Comment:
                            // Preserve user indent in argument lists
                            preserveUserIndent = _braceHandler.IsInArguments();
                            break;

                        default:
                            // If expression between scope start and the current point is incomplete,
                            // (i.e. we are in a middle of multiline expression) we want to preserve 
                            // user-supplied indentation.
                            preserveUserIndent = !_expressionHelper.IsCompleteExpression(_tokens.Position);
                            break;
                    }

                    // Also preserve indent before the tokens below. This matter in long lists 
                    // of arguments in functions or indexers and also before comments.
                    if (!preserveUserIndent) {
                        switch (_tokens.CurrentToken.TokenType) {
                            case RTokenType.CloseBrace:
                            case RTokenType.CloseSquareBracket:
                            case RTokenType.CloseDoubleSquareBracket:
                            case RTokenType.Comma:
                            case RTokenType.Operator:
                            case RTokenType.Comment:
                                preserveUserIndent = true;
                                break;
                        }
                    }
                }

                // Preserve line breaks user has entered.
                _tb.CopyPrecedingLineBreaks(_textProvider, end);

                if (preserveUserIndent) {
                    // Construct indentation line based on the size of the user indent
                    // but using tabs or spaces per formatting options.
                    int lastLineBreakIndex = text.LastIndexOfAny(CharExtensions.LineBreakChars);
                    if (lastLineBreakIndex >= 0) {
                        text = text.Substring(lastLineBreakIndex + 1);
                        int textIndentInSpaces = IndentBuilder.TextIndentInSpaces(text, _options.TabSize);
                        text = IndentBuilder.GetIndentString(textIndentInSpaces, _indentBuilder.IndentType, _indentBuilder.TabSize);
                        _tb.AppendPreformattedText(text);
                    }
                } else {
                    _tb.SoftIndent();
                }
            } else {
                // If there is unrecognized text between tokens, append it verbatim.
                _tb.AppendPreformattedText(text);
            }
        }

        private void AppendComma() {
            bool trailingSpace;
            if (IsClosingToken(_tokens.NextToken)) {
                trailingSpace = false;
            } else {
                trailingSpace = _options.SpaceAfterComma;
            }

            AppendToken(leadingSpace: false, trailingSpace: trailingSpace);
        }

        private bool ShouldAppendTextBeforeToken() {
            if (_tokens.PreviousToken.TokenType == RTokenType.Comment &&
                _tokens.CurrentToken.TokenType != RTokenType.Comment) {
                // Copy any possible indentation after comment
                // at the previous line such as in 
                //    func(a, #comment
                //         b
                // Do not do this when comment follows another comment
                // since otherwise they won't align.
                //
                return true;
            }

            // We position curly braces according to formatting options
            // hence whitespace before { doesn't matter
            if (_tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace) {
                return false;
            }

            // If user added like break before command, ), ] or ]] leave it alone.
            if (IsClosingToken(_tokens.CurrentToken) && !_tokens.IsLineBreakAfter(_textProvider, _tokens.Position - 1)) {
                return false;
            }

            return true;
        }

        private bool HasSameLineElse() {
            bool foundSameLineElse = false;

            for (int i = _tokens.Position; !foundSameLineElse && i < _tokens.Length; i++) {
                if (_tokens[i].TokenType == RTokenType.OpenCurlyBrace || _tokens.IsLineBreakAfter(_textProvider, i)) {
                    break;
                }

                if (_tokens[i].TokenType == RTokenType.Keyword) {
                    foundSameLineElse = _tokens[i].IsKeywordText(_textProvider, "else");
                    break; // any keyword breaks if/else sequence as in if(TRUE) if(FALSE) ...
                }
            }

            return foundSameLineElse;
        }

        /// <summary>
        /// Tokenizes provided string that contains R code
        /// </summary>
        private void Tokenize(string text) {
            _textProvider = new TextStream(text);

            var tokenizer = new RTokenizer(separateComments: false);
            var tokens = tokenizer.Tokenize(_textProvider, 0, _textProvider.Length);
            _tokens = new TokenStream<RToken>(tokens, RToken.EndOfStreamToken);

            _braceHandler = new BraceHandler(_tokens, _tb);
            _expressionHelper = new ExpressionHelper(_tokens, _textProvider);
        }

        /// <summary>
        /// Calculates position of a end of a single-line scope.
        /// The simple scope ends at a line break or at opening 
        /// or closing curly brace.
        /// </summary>
        /// <returns></returns>
        private int GetSingleLineScopeEnd() {
            int closeBraceIndex = TokenBraceCounter<RToken>.GetMatchingBrace(
                      _tokens,
                      new RToken(RTokenType.OpenCurlyBrace),
                      new RToken(RTokenType.CloseCurlyBrace),
                      new RTokenTypeComparer());

            int end = closeBraceIndex < 0 ? _textProvider.Length : _tokens[closeBraceIndex].End;
            for (int i = _tokens.CurrentToken.End; i < end; i++) {
                if (CharExtensions.IsLineBreak(_textProvider[i])) {
                    return -1;
                }
            }

            return end;
        }

        private bool SingleLineScope => _singleLineScopeEnd >= 0;

        private void SoftLineBreak() {
            if (!SingleLineScope) {
                _tb.SoftLineBreak();
            }
        }
    }
}
