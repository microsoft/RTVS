// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST {
    public static class AstNodeExtensions {
        /// <summary>
        /// Locates enclosing scope for a given node
        /// </summary>
        public static IScope GetScope(this IAstNode node) {
            var n = node.Parent;
            while (!(n is IScope)) {
                node = node.Parent;
            }
            return n as IScope;
        }
    }
}
