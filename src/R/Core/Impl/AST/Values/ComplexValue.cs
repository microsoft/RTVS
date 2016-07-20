// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values {
    /// <summary>
    /// Represents complex number
    /// </summary>
    public sealed class ComplexValue : RValueTokenNode<RComplex>, ILiteralNode {
        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            double realPart = Double.NaN;
            double imaginaryPart = Double.NaN;

            Debug.Assert(currentToken.TokenType == RTokenType.Complex);

            // Split into real and imaginary parts. Imaginary part
            // should always be there since otherwise tokenizer would
            // not have idenfified the number as complex. Note that 
            // real part may be missing as in '+0i'. Operator may also
            // be missing: 1i is a legal complex number.

            Debug.Assert(text[text.Length - 1] == 'i');

            // Drop trailing i and retokenize as two numbers
            RTokenizer tokenizer = new RTokenizer(separateComments: false);
            IReadOnlyTextRangeCollection<RToken> tokens = tokenizer.Tokenize(text.Substring(0, text.Length - 1));

            if (tokens.Count == 1) {
                // Only imaginary part is present
                Debug.Assert(tokens[0].TokenType == RTokenType.Number);
                if (!Number.TryParse(text.Substring(tokens[0].Start, tokens[0].Length), out imaginaryPart)) {
                    imaginaryPart = Double.NaN;
                }
            } else if (tokens.Count == 3) {
                // Real and imaginary parts present
                Debug.Assert(tokens[0].TokenType == RTokenType.Number);
                Debug.Assert(tokens[1].TokenType == RTokenType.Operator);
                Debug.Assert(tokens[2].TokenType == RTokenType.Number);

                string real = text.Substring(tokens[0].Start, tokens[0].Length);
                if (!Number.TryParse(real, out realPart)) {
                    realPart = Double.NaN;
                }
                // Imaginary does not allow 'L' suffix
                string imaginary = text.Substring(tokens[2].Start, tokens[2].Length);
                if (!Number.TryParse(imaginary, out imaginaryPart, allowLSuffix: false)) {
                    imaginaryPart = Double.NaN;
                }
            }

            if (realPart == Double.NaN || imaginaryPart == Double.NaN) {
                context.AddError(new MissingItemParseError(ParseErrorType.NumberExpected, context.Tokens.PreviousToken));
                return false;
            }

            Complex complex = new Complex(realPart, imaginaryPart);
            Value = new RComplex(complex);
            return base.Parse(context, parent);
        }
    }
}
