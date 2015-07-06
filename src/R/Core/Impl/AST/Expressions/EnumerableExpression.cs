using System;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions
{
    /// <summary>
    /// Represents inner part of 'for' operator which looks like 'for ( name in vector )'.
    /// http://cran.r-project.org/doc/manuals/r-release/R-lang.html#for
    /// </summary>
    public sealed class EnumerableExpression : AstNode
    {
        public TokenNode VariableName { get; private set; }
        public TokenNode InOperator { get; private set; }
        public Expression Expression { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            TokenStream<RToken> tokens = context.Tokens;

            if(tokens.CurrentToken.TokenType == RTokenType.Identifier)
            {
                this.VariableName = new TokenNode();
                this.VariableName.Parse(context, this);

                if(tokens.CurrentToken.TokenType == RTokenType.Keyword)
                {
                    string keyword = context.TextProvider.GetText(tokens.CurrentToken);
                    if(keyword.Equals("in", StringComparison.Ordinal))
                    {
                        this.InOperator = new TokenNode();
                        this.InOperator.Parse(context, this);

                        this.Expression = new Expression();
                        if(this.Expression.Parse(context, this))
                        {
                            return base.Parse(context, parent);
                        }
                    }
                    else
                    {
                        context.Errors.Add(new ParseError(ParseErrorType.InKeywordExpected, tokens.CurrentToken));
                    }
                }
            }
            else
            {
                context.Errors.Add(new ParseError(ParseErrorType.IndentifierExpected, tokens.CurrentToken));
            }

            return false;
        }
    }
}
