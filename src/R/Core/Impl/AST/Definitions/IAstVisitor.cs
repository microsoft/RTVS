// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST {
    /// <summary>
    /// Implemented by code that needs to traverse AST 
    /// using standard visitor design pattern.
    /// https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public interface IAstVisitor {
        /// <summary>
        /// Called by the tree traversal code.
        /// </summary>
        /// <param name="node">Node that is being visited</param>
        /// <param name="parameter">Arbitrary data passed to the visitor pattern call</param>
        /// <returns>True if tree traversal should continue, false otherwise</returns>
        bool Visit(IAstNode node, object parameter);
    }
}
