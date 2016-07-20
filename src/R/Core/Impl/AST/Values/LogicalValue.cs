// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values {
    /// <summary>
    /// Represents logical constant (TRUE or FALSE)
    /// </summary>
    public sealed class LogicalValue : RValueTokenNode<RLogical>, ILiteralNode {
        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);
            bool? result = null;

            Debug.Assert(currentToken.TokenType == RTokenType.Logical);

            if (text.Equals("TRUE", StringComparison.Ordinal) || text.Equals("T", StringComparison.Ordinal)) {
                result = true;
            } else if (text.Equals("FALSE", StringComparison.Ordinal) || text.Equals("F", StringComparison.Ordinal)) {
                result = false;
            }

            if (result.HasValue) {
                Value = new RLogical(result.Value);
                return base.Parse(context, parent);
            }

            context.AddError(new ParseError(ParseErrorType.LogicalExpected, ErrorLocation.Token, currentToken));
            return false;
        }
    }
}
