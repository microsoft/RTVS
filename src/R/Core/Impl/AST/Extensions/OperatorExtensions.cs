// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators.Definitions;

namespace Microsoft.R.Core.AST {
    public static class OperatorExtensions {
        public static IFunctionDefinition GetFunctionDefinition(this IOperator op) {
            var exp = op.RightOperand as IExpression;
            if (exp != null && exp.Children.Count == 1) {
                return exp.Children[0] as IFunctionDefinition;
            }
            return null;
        }
    }
}
