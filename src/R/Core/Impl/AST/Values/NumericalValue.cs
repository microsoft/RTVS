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

            if (text[text.Length - 1] == 'L')
            {
                int r;
                if (Int32.TryParse(text.Substring(0, text.Length-1), out r))
                {
                    NodeValue = new RNumber(r);
                    return base.Parse(context, parent);
                }
            }
            else
            {
                if (Double.TryParse(text, out result))
                {
                    NodeValue = new RNumber(result);
                    return base.Parse(context, parent);
                }
            }

            context.AddError(new ParseError(ParseErrorType.NumberExpected, ParseErrorLocation.Token, currentToken));
            return false;
        }
    }
}
