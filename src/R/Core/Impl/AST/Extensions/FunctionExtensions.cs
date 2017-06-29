// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST.Extensions {
    public static class FunctionExtensions {
        public static string GetFunctionName(this FunctionCall fc)
            => (fc?.RightOperand as IVariable)?.Name;

        public static FunctionCall GetFunctionCall(this IExpression exp) {
            if (exp?.Children != null && exp.Children.Count == 1) {
                return exp.Children[0] as FunctionCall;
            }
            return null;
        }
    }
}
