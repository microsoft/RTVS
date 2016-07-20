// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Statements.Conditionals;

namespace Microsoft.R.Core.AST.Operands {
    /// <summary>
    /// Represents inline conditional which is basically a scope-less
    /// if/else statement but in the inline case it acts like a function
    /// call and hence can be used as an operand in function calls and
    /// other expressions such as 'func(if (a > b) x else y, c, d = 0)'
    /// </summary>
    public sealed class InlineIf : If, IRValueNode {
        public RObject Value { get; set; }
    }
}
