using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Functions
{
    public sealed class FunctionDefinition : AstNode, IFunctionDefinition
    {
        #region IFunctionDefinition
        public TokenNode Keyword { get; private set; }

        public TokenNode OpenBrace { get; private set; }

        public ArgumentList Arguments { get; private set; }

        public TokenNode CloseBrace { get; private set; }

        public IScope Scope { get; private set; }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.Keyword);
            this.Keyword = RParser.ParseKeyword(context, this);

            if (tokens.CurrentToken.TokenType == RTokenType.OpenBrace)
            {
                this.OpenBrace = RParser.ParseToken(context, this);

                this.Arguments = new ArgumentList(RTokenType.CloseBrace);
                if (this.Arguments.Parse(context, this))
                {
                    if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace)
                    {
                        this.CloseBrace = RParser.ParseToken(context, this);
                        this.Scope = RParser.ParseScope(context, this, allowsSimpleScope: true, terminatingKeyword: null);
                        if (this.Scope != null)
                        {
                            return base.Parse(context, parent);
                        }
                    }

                    context.AddError(new MissingItemParseError(ParseErrorType.CloseBraceExpected, tokens.PreviousToken));
                }
            }
            else
            {
                context.AddError(new MissingItemParseError(ParseErrorType.OpenBraceExpected, tokens.PreviousToken));
            }

            return false;
        }
    }
}
