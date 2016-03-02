// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Expressions.Definitions {
    /// <summary>
    /// Represents mathematical or conditional expression, 
    /// assignment, function or operator definition optionally
    /// enclosed in braces. Expression is a tree and may have
    /// nested extressions in its content.
    /// </summary>
    public interface IExpression : IAstNode {
        IRValueNode Content { get; }
    }
}
