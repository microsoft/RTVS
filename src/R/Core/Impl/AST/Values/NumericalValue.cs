// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values {
    public sealed class NumericalValue : RValueTokenNode<RNumber>, ILiteralNode {
        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            double result;

            Debug.Assert(currentToken.TokenType == RTokenType.Number ||
                         currentToken.TokenType == RTokenType.Infinity ||
                         currentToken.TokenType == RTokenType.NaN);

            if (currentToken.TokenType == RTokenType.Infinity) {
                Value = new RNumber(Double.PositiveInfinity);
             } else if (currentToken.TokenType == RTokenType.NaN) {
                Value = new RNumber(Double.NaN);
            } else {
                // If parsing fails we still need to create node
                // since we need a range to squiggle
                result = Double.NaN;
                if (!Number.TryParse(text, out result)) {
                    // Something unparsable
                    context.AddError(new ParseError(ParseErrorType.NumberExpected, ErrorLocation.Token, currentToken));
                }
                Value = new RNumber(result);
            }
            return base.Parse(context, parent);
        }
    }
}
