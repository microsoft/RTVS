// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    public interface IAstNodeWithScope : IAstNode {
        IScope Scope { get; }
    }
}
