// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements {
    [DebuggerDisplay("[KeywordExpressionStatement: {Text}]")]
    public class KeywordExpressionStatement : KeywordStatement, IKeywordExpression {
        #region IKeywordExpression
        public TokenNode OpenBrace { get; private set; }
        public IExpression Expression { get; private set; }
        public TokenNode CloseBrace { get; private set; }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (ParseKeyword(context, parent)) {
                this.OpenBrace = RParser.ParseOpenBraceSequence(context, this);
                if (this.OpenBrace != null) {
                    this.ParseExpression(context, this);

                    // Even if expression is broken but we are at 
                    // the closing brace we want to recover and continue.

                    if (context.Tokens.CurrentToken.TokenType == Tokens.RTokenType.CloseBrace) {
                        this.CloseBrace = RParser.ParseCloseBraceSequence(context, this);
                        if (this.CloseBrace != null) {
                            this.Parent = parent;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Allows derived classes to parse expression inside braces
        /// </summary>
        protected virtual bool ParseExpression(ParseContext context, IAstNode parent) {
            this.Expression = new Expression(inGroup: true);
            return this.Expression.Parse(context, this);
        }
    }
}