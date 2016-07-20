// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions {
    /// <summary>
    /// Represents empty expression such as a missing
    /// value in a function call: 'func(a=).
    /// </summary>
    [DebuggerDisplay("NullExpression [{Start}...{End})")]
    public sealed class NullExpression : RValueNode, IExpression {
        #region IExpression
        public TokenNode OpenBrace { get; private set; }
        public IRValueNode Content { get; private set; }
        public TokenNode CloseBrace { get; private set; }
        #endregion

        public NullExpression() {
            Value = RNull.Null;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (context.Tokens.CurrentToken.TokenType == RTokenType.OpenBrace) {
                context.AddError(new ParseError(ParseErrorType.UnexpectedToken, ErrorLocation.Token, context.Tokens.CurrentToken));
            }

            return false;
        }
    }
}
