using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments
{
    /// <summary>
    /// Argument to a function or to indexer
    /// </summary>
    [DebuggerDisplay("[{Count}]")]
    public sealed class ArgumentList : CommaSeparatedList
    {
        public ArgumentList(RTokenType terminatingTokenType) :
            base(terminatingTokenType)
        {
        }

        protected override IAstNode CreateItem(IAstNode parent, ParseContext context)
        {
            RToken currentToken = context.Tokens.CurrentToken;
            RToken nextToken = context.Tokens.NextToken;

            if (currentToken.TokenType == RTokenType.Ellipsis)
            {
                return new EllipsisArgument();
            }

            if (currentToken.TokenType == RTokenType.Comma)
            {
                return new MissingArgument();
            }

            if (currentToken.TokenType == RTokenType.Identifier &&
                nextToken.TokenType == RTokenType.Operator &&
                context.TextProvider.GetText(nextToken) == "=")
            {
                return new NamedArgument();
            }

            return new ExpressionArgument();
        }
    }
}
