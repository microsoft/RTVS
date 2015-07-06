using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Parser.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser
{
    [DebuggerDisplay("{Tokens.Position} = {Tokens.CurrentToken.TokenType} : Errors = {Errors.Count}")]
    public sealed class ParseContext
    {
        public ITextProvider TextProvider { get; private set; }
        public TokenStream<RToken> Tokens { get; private set; }
        public ITextRange TextRange { get; private set; }
        public List<IParseError> Errors { get; private set; }

        public ParseContext(ITextProvider textProvider, ITextRange range, TokenStream<RToken> tokens)
        {
            this.TextProvider = textProvider;
            this.Tokens = tokens;
            this.TextRange = range;
            this.Errors = new List<IParseError>();
        }

        public void RemoveCommentTokens()
        {
            List<RToken> comments = new List<RToken>();
            List<RToken> filteredStream = new List<RToken>();

            foreach (RToken token in this.Tokens)
            {
                if (token.TokenType == RTokenType.Comment)
                {
                    comments.Add(token);
                }
                else
                {
                    filteredStream.Add(token);
                }
            }

            this.Tokens = new TokenStream<RToken>(new ReadOnlyTextRangeCollection<RToken>(
                                                       new TextRangeCollection<RToken>(filteredStream)), 
                                                       RToken.EndOfStreamToken);
        }
    }
}
