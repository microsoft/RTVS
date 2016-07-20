// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values {
    /// <summary>
    /// Represents string constant
    /// </summary>
    public sealed class StringValue : RValueTokenNode<RString>, ILiteralNode {
        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);

            Debug.Assert(currentToken.TokenType == RTokenType.String);

            Value = new RString(text);
            return base.Parse(context, parent);
        }
    }
}
