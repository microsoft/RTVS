// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements.Loops {
    /// <summary>
    /// For statement
    /// </summary>
    [DebuggerDisplay("[For Statement]")]
    public class For : KeywordExpressionScopeStatement {

        public IEnumerableExpression EnumerableExpression { get; private set; }

        protected override bool ParseExpression(ParseContext context, IAstNode parent) {
            this.EnumerableExpression = new EnumerableExpression();
            return this.EnumerableExpression.Parse(context, this);
        }
    }
}
