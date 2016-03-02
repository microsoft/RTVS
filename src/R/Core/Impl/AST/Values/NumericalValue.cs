// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values {
    public sealed class NumericalValue : RValueTokenNode<RNumber> {
        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            double result;

            Debug.Assert(currentToken.TokenType == RTokenType.Number ||
                         currentToken.TokenType == RTokenType.Infinity ||
                         currentToken.TokenType == RTokenType.NaN);

            if (currentToken.TokenType == RTokenType.Infinity) {
                NodeValue = new RNumber(Double.PositiveInfinity);
             } else if (currentToken.TokenType == RTokenType.NaN) {
                NodeValue = new RNumber(Double.NaN);
            } else {
                if (text[text.Length - 1] == 'L') {
                    text = text.Substring(0, text.Length - 1);
                }
                // If parsing fails we still need to create node
                // since we need a range to squiggle
                result = 0.0;
                if (!Double.TryParse(text, out result)) {
                    // Something unparsable
                    result = Double.NaN;
                    context.AddError(new ParseError(ParseErrorType.NumberExpected, ErrorLocation.Token, currentToken));
                }
                NodeValue = new RNumber(result);
            }
            return base.Parse(context, parent);
        }
    }
}
