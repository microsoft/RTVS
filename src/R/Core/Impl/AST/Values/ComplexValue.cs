using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values
{
    /// <summary>
    /// Represents complex number
    /// </summary>
    public sealed class ComplexValue : RValueTokenNode<RComplex>
    {
        public override bool Parse(ParseContext context, IAstNode parent)
        {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            double realPart = 0;
            double imaginaryPart = 0;

            Debug.Assert(currentToken.TokenType == RTokenType.Complex);

            // Split into real and imaginary parts. Imaginary part
            // should always be there since otherwise tokenizer would
            // not have idenfified the number as complex. Note that 
            // real part may be missing as in '+0i'. Operator may also
            // be missing: 1i is a legal complex number.

            Debug.Assert(text[text.Length - 1] == 'i');

            // Drop trailing i and retokenize as two numbers
            RTokenizer tokenizer = new RTokenizer();
            IReadOnlyTextRangeCollection<RToken> tokens= tokenizer.Tokenize(text.Substring(0, text.Length - 1));

            if(tokens.Count == 1)
            {
                // Only imaginary part is present
                Debug.Assert(tokens[0].TokenType == RTokenType.Number);
                if(!Double.TryParse(text.Substring(tokens[0].Start, tokens[0].Length), out imaginaryPart))
                {
                    return false;
                }
            }
            else if(tokens.Count == 3)
            {
                // Real and imaginary parts present
                Debug.Assert(tokens[0].TokenType == RTokenType.Number);
                Debug.Assert(tokens[1].TokenType == RTokenType.Operator);
                Debug.Assert(tokens[2].TokenType == RTokenType.Number);

                if (!Double.TryParse(text.Substring(tokens[0].Start, tokens[0].Length), out realPart)
                    || !Double.TryParse(text.Substring(tokens[2].Start, tokens[2].Length), out imaginaryPart))
                {
                    return false;
                }
            }
            else
            {
                context.AddError(new MissingItemParseError(ParseErrorType.NumberExpected, context.Tokens.PreviousToken));
                return false;
            }

            Complex complex = new Complex(realPart, imaginaryPart);
            NodeValue = new RComplex(complex);
            return base.Parse(context, parent);
        }
    }
}
