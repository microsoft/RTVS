// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Scopes {
    /// <summary>
    /// Represents a sequence of statements typically under control 
    /// of a parent statement such as for(...) { }. Statements may
    /// be enclosed in curly braces in which case scope can also 
    /// declare new local variables and functions.
    /// </summary>
    public interface IScope : IAstNode {
        string Name { get; }

        TokenNode OpenCurlyBrace { get; }

        TokenNode CloseCurlyBrace { get; }

        /// <summary>
        /// Tells that block is a KnitR code chunk options block
        /// </summary>
        bool KnitrOptions { get; }
    }
}
