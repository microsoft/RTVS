// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Statements;

namespace Microsoft.R.Core.AST.Functions {
    public interface IFunctionDefinition : IFunction, IKeyword, IAstNodeWithScope, IRValueNode {
    }
}
