// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST.Expressions {
    /// <summary>
    /// Represents expression that is used in enumerations
    /// such as in 'for(x in exp) { }'. Enumerable expressions
    /// do not allow braces and cannot be nested.
    /// </summary>
    public interface IEnumerableExpression : IAstNode {
        /// <summary>
        /// Variable in 'for(variable_name in ...)'
        /// </summary>
        IVariable Variable { get; }

        /// <summary>
        /// Token of the 'in' operator
        /// </summary>
        TokenNode InOperator { get; }

        /// <summary>
        /// Expression in 'for(variable_name in expression)'
        /// </summary>
        IExpression Expression { get; }
    }
}
