// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Functions;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents statement that consists of a function.
    /// Typically used in inline if statement scopes
    /// </summary>
    [DebuggerDisplay("[FunctionStatement]")]
    public sealed class FunctionStatement : FunctionDefinition, IStatement {
        /// <summary>
        /// Optional terminating semicolon
        /// </summary>
        public TokenNode Semicolon { get; private set; }
    }
}
