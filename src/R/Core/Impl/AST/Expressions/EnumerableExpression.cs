// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions {
    /// <summary>
    /// Represents inner part of 'for' operator which looks like 'for ( name in vector )'.
    /// http://cran.r-project.org/doc/manuals/r-release/R-lang.html#for
    /// </summary>
    public sealed class EnumerableExpression : AstNode, IEnumerableExpression {
        #region IEnumerableExpression
        /// <summary>
        /// Name of variable in 'for(variable_name in ...)'
        /// </summary>
        public IVariable Variable { get; private set; }

        /// <summary>
        /// Token of the 'in' operator
        /// </summary>
        public TokenNode InOperator { get; private set; }

        /// <summary>
        /// Expression in 'for(variable_name in expression)'
        /// </summary>
        public IExpression Expression { get; private set; }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            if (tokens.CurrentToken.IsVariableKind()) {
                var v = new Variable();
                v.Parse(context, this);

                // Variables don't set parent since during complex
                // exression parsing parent is determined by the
                // expression parser based on precedence and grouping.
                v.Parent = this; 
                this.Variable = v;

                if (tokens.CurrentToken.IsKeywordText(context.TextProvider, "in")) {
                    this.InOperator = new TokenNode();
                    this.InOperator.Parse(context, this);

                    this.Expression = new Expression(inGroup: true);
                    if (this.Expression.Parse(context, this)) {
                        return base.Parse(context, parent);
                    }
                } else {
                    context.AddError(new MissingItemParseError(ParseErrorType.InKeywordExpected, tokens.CurrentToken));
                }
            } else {
                context.AddError(new MissingItemParseError(ParseErrorType.IndentifierExpected, tokens.PreviousToken));
            }

            return false;
        }
    }
}
