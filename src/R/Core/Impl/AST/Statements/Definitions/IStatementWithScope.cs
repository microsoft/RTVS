// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Scopes;

namespace Microsoft.R.Core.AST.Statements {
    public interface IAstNodeWithScope : IAstNode {
        IScope Scope { get; }
    }
}
