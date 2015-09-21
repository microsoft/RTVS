using System.Diagnostics;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting
{
    /// <summary>
    /// Settings for formatting of { } scope.
    /// </summary>
    internal sealed class FormattingScope
    {
        private IndentState _previousState;
        private IndentBuilder _indentBuilder;

        public int CloseBracePosition { get; private set; } = -1;

        public int SuppressLineBreakCount { get; set; }

        public FormattingScope(IndentBuilder indentBuilder)
        {
            _indentBuilder = indentBuilder;
        }

        public void Close()
        {
            if (_previousState != null)
            {
                _indentBuilder.RestoreIndentState(_previousState);
            }
        }

        public bool Open(ITextProvider textProvider, TokenStream<RToken> tokens, RFormatOptions options)
        {
            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace);

            // When formatting scope in function arguments, use user indent
            // where appropriate. User indent can be determined by 
            //  a. current indentation of { if 'braces on new line' is on
            //     and the open brace indent is deeper than the default.
            //  b. if the above does not apply, it is equal to the indent
            //     of the previous line.

            // System.Action x = () => {
            // }; <<- equal to the statement indent.

            // System.Action x = () => 
            // { <<- equal to the statement indent.
            // };

            // System.Action x = () => 
            //      {
            //      }; <<- based on the *opening* brace position.

            CloseBracePosition = TokenBraceCounter<RToken>.GetMatchingBrace(tokens,
                new RToken(RTokenType.OpenCurlyBrace), new RToken(RTokenType.CloseCurlyBrace), new RTokenTypeComparer());

            //if (CloseBracePosition > 0)
            //{
            //    if (!IsLineBreakInRange(textProvider, tokens.CurrentToken.End, tokens[CloseBracePosition].Start))
            //    {
            //        SuppressLineBreakCount++;
            //        return true;
            //    }
            //}

            if (options.BracesOnNewLine)
            {
                // If open curly is on its own line (there is only whitespace
                // between line break and the curly, find out current indent
                // and if it is deeper than the default one, use it,
                // otherwise continue with default.
                CompareAndSetIndent(textProvider, tokens, tokens.CurrentToken.Start, options);
                return true;
            }

            return false;
        }

        private void CompareAndSetIndent(ITextProvider textProvider, TokenStream<RToken> tokens, int position, RFormatOptions options)
        {
            // If curly is on its own line (there is only whitespace between line break 
            // and the curly, find out its current indent and if it is deeper than 
            // the default one, use it, otherwise continue with default.

            string userIndentString = GetUserIndentString(textProvider, position, options);
            int defaultIndentSize = _indentBuilder.IndentLevelString.Length;
            if (userIndentString.Length > defaultIndentSize)
            {
                _previousState = _indentBuilder.ResetBaseIndent(userIndentString);
            }
        }

        private string GetUserIndentString(ITextProvider textProvider, int position, RFormatOptions options)
        {
            for (int i = position - 1; i >= 0; i--)
            {
                char ch = textProvider[i];
                if (!char.IsWhiteSpace(ch))
                {
                    break;
                }

                if (ch == '\n' || ch == '\r')
                {
                    string userIndentString = textProvider.GetText(TextRange.FromBounds(i + 1, position));
                    int indentSize = IndentBuilder.TextIndentInSpaces(userIndentString, options.TabSize);
                    return IndentBuilder.GetIndentString(indentSize, options.IndentType, options.IndentSize);
                }
            }

            return string.Empty;
        }

        private bool IsLineBreakInRange(ITextProvider textProvider, int start, int end)
        {
            return textProvider.IndexOf('\n', TextRange.FromBounds(start, end)) >= 0;
        }
    }
}
