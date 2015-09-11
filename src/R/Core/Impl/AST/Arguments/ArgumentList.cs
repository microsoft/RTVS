using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments
{
    /// <summary>
    /// Arguments of a function or to indexer.
    /// Does not include outer braces. Each argument is
    /// an expression. Allows for missing arguments. 
    /// Examples are 'a, b[3], c+2' or '1,,3,...'
    /// </summary>
    [DebuggerDisplay("Arguments: {Count} [{Start}...{End})")]
    public sealed class ArgumentList : CommaSeparatedList
    {
        public ArgumentList(RTokenType terminatingTokenType) :
            base(terminatingTokenType)
        {
        }

        protected override CommaSeparatedItem CreateItem(IAstNode parent, ParseContext context)
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

            if(currentToken.TokenType == RTokenType.CloseBrace)
            {
                return null; // no arguments supplied
            }

            return new ExpressionArgument();
        }
    }
}
