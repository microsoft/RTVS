// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {
    // https://en.wikipedia.org/wiki/Operator_associativity
    /// <summary>
    /// Associativity. Consider the expression a~b~c. If the operator 
    /// ~ has left associativity, this expression would be interpreted as (a~b)~c.
    /// If the operator has right associativity, the expression would be
    /// interpreted as a~(b~c).
    /// </summary>
    public enum Associativity {
        /// <summary>
        /// Left associativity, the expression is interpreted as (a~b)~c
        /// </summary>
        Left,
        /// <summary>
        /// Right associativity, the expression is interpreted as a~(b~c)
        /// </summary>
        Right
    }
}
