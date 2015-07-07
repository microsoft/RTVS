using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values
{
    public sealed class NumericalValue : RValueTokenNode<RNumber>
    {
        public override bool Parse(ParseContext context, IAstNode parent)
        {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            double result;

            Debug.Assert(currentToken.TokenType == RTokenType.Number);

            if (Double.TryParse(text, out result))
            {
                NodeValue = new RNumber(result);
                return base.Parse(context, parent);
            }

            context.Errors.Add(new ParseError(ParseErrorType.NumberExpected, currentToken));
            return false;
        }
    }
}
