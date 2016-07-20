// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.AST {
    /// <summary>
    /// Implements standard visitor pattern on the AST.
    /// https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public interface IAstVisitorPattern {
        /// <summary>
        /// Traverses the entire tree invoking provided visitor interface.
        /// Returns true if the entire tree was traversed. Visitor can cancel 
        /// the traversal at any time by returning false from the callback.
        /// </summary>
        bool Accept(IAstVisitor visitor, object parameter);

        /// <summary>
        /// Traverses the entire tree invoking provided visitor function.
        /// Returns true if the entire tree was traversed. Visitor can cancel 
        /// the traversal at any time by returning false from the callback.
        /// </summary>
        bool Accept(Func<IAstNode, object, bool> visitor, object parameter);
    }
}
