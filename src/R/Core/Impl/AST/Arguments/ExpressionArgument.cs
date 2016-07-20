// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Arguments {
    [DebuggerDisplay("Expression Argument [{Start}...{End})")]
    public class ExpressionArgument : CommaSeparatedItem {
        public Expression ArgumentValue { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent) {
            this.ArgumentValue = new Expression(inGroup: true);
            if (this.ArgumentValue.Parse(context, this)) {
                return base.Parse(context, parent);
            }

            return false;
        }
    }
}
