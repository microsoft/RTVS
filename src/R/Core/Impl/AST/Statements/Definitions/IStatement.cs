// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents a statement with optional terminating semicolon.
    /// </summary>
    public interface IStatement : IAstNode {
        /// <summary>
        /// Optional terminating semicolon
        /// </summary>
        TokenNode Semicolon { get; }
    }
}
