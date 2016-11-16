// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Variables {
    /// <summary>
    /// Indexer operator. Applies to a variable if it is 
    /// a direct call like name[1] or to a result
    /// of another similar operator such as indexer
    /// or function call as in x(a)[1] or x[1][a].
    /// </summary>
    [DebuggerDisplay("Indexer, Args:{Arguments.Count} [{Start}...{End})")]
    public sealed class Indexer : Operator {
        public TokenNode LeftBrackets { get; private set; }
        public ArgumentList Arguments { get; private set; }
        public TokenNode RightBrackets { get; private set; }

        #region IOperator
        public override OperatorType OperatorType => OperatorType.Index;
        #endregion

        public Indexer() {
            IsUnary = true;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenSquareBracket ||
                         tokens.CurrentToken.TokenType == RTokenType.OpenDoubleSquareBracket);

            this.LeftBrackets = RParser.ParseToken(context, this);
            RTokenType terminatingTokenType = RParser.GetTerminatingTokenType(this.LeftBrackets.Token.TokenType);

            this.Arguments = new ArgumentList(terminatingTokenType);
            this.Arguments.Parse(context, this);

            if (tokens.CurrentToken.TokenType == terminatingTokenType) {
                this.RightBrackets = RParser.ParseToken(context, this);
            } else {
                context.AddError(new MissingItemParseError(ParseErrorType.CloseSquareBracketExpected, tokens.PreviousToken));
            }

            return base.Parse(context, parent);
        }
    }
}
