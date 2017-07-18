// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Scopes;

namespace Microsoft.R.Core.AST {
    public static class AstNodeExtensions {
        /// <summary>
        /// Locates enclosing scope for a given node
        /// </summary>
        public static IScope GetEnclosingScope(this IAstNode node) {
            if (node is GlobalScope gs) {
                return gs;
            }

            if (node is AstRoot root) {
                return root.Children.Count > 0 ? root.Children[0] as IScope : null;
            }

            var n = node.Parent;
            while (!(n is IScope) && n != null) {
                n = n.Parent;
            }
            return n as IScope;
        }
    }
}
