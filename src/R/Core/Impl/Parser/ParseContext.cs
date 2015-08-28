using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser
{
    [DebuggerDisplay("{Tokens.Position} = {Tokens.CurrentToken.TokenType} : Errors = {Errors.Count}")]
    public sealed class ParseContext
    {
        private List<IParseError> _errors = new List<IParseError>();
        private List<RToken> _comments = new List<RToken>();

        public AstRoot AstRoot { get; private set; }

        public ITextProvider TextProvider { get; private set; }

        public TokenStream<RToken> Tokens { get; private set; }

        public ITextRange TextRange { get; private set; }

        public IReadOnlyCollection<IParseError> Errors
        {
            get { return _errors; }
        }

        public IReadOnlyCollection<RToken> Comments
        {
            get { return _comments; }
        }

        public ParseContext(ITextProvider textProvider, ITextRange range, TokenStream<RToken> tokens)
        {
            this.AstRoot = new AstRoot(textProvider);
            this.TextProvider = textProvider;
            this.Tokens = tokens;
            this.TextRange = range;
        }

        public void AddError(ParseError error)
        {
            bool found = false;

            foreach(IParseError e in _errors)
            {
                if(e.Start == error.Start && e.Length == error.Length && e.ErrorType == error.ErrorType)
                {
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                _errors.Add(error);
            }
        }

        public void RemoveCommentTokens()
        {
            _comments = new List<RToken>();
            List<RToken> filteredStream = new List<RToken>();

            foreach (RToken token in this.Tokens)
            {
                if (token.TokenType == RTokenType.Comment)
                {
                    _comments.Add(token);
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
