using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values
{
    /// <summary>
    /// Represents logical constant (TRUE or FALSE)
    /// </summary>
    public sealed class LogicalValue : RValueTokenNode<RLogical>
    {
        public override bool Parse(ParseContext context, IAstNode parent)
        {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            bool? result = null;

            Debug.Assert(currentToken.TokenType == RTokenType.Logical);

            if (text.Equals("TRUE", StringComparison.Ordinal) || text.Equals("T", StringComparison.Ordinal))
            {
                result = true;
            }
            else if (text.Equals("FALSE", StringComparison.Ordinal) || text.Equals("F", StringComparison.Ordinal))
            {
                result = false;
            }

            if (result.HasValue)
            {
                this.nodeValue = new RLogical(result.Value);
                return base.Parse(context, parent);
            }

            context.Errors.Add(new ParseError(ParseErrorType.LogicalExpected, currentToken));
            return false;
        }
    }
}
