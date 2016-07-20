// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements {
    [DebuggerDisplay("[KeywordExpression: {Text}]")]
    public class KeywordExpression : AstNode, IKeywordExpression {
        #region IKeywordExpression
        public TokenNode OpenBrace { get; private set; }
        public IExpression Expression { get; private set; }
        public TokenNode CloseBrace { get; private set; }
        #endregion

        #region IKeyword
        public TokenNode Keyword { get; private set; }
        public string Text { get; private set; }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (ParseKeyword(context, parent)) {

                this.OpenBrace = RParser.ParseOpenBraceSequence(context, this);
                if (this.OpenBrace != null) {
                    this.Expression = new Expression(inGroup: true);
                    this.Expression.Parse(context, this);

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

        protected bool ParseKeyword(ParseContext context, IAstNode parent) {
            this.Keyword = RParser.ParseKeyword(context, this);
            this.Text = context.TextProvider.GetText(this.Keyword);

            return true;
        }
    }
}