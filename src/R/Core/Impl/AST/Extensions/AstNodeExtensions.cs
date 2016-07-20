// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Scopes;

namespace Microsoft.R.Core.AST {
    public static class AstNodeExtensions {
        /// <summary>
        /// Locates enclosing scope for a given node
        /// </summary>
        public static IScope GetEnclosingScope(this IAstNode node) {
            var gs = node as GlobalScope;
            if (gs != null) {
                return gs;
            }

            var root = node as AstRoot;
            if (root != null) {
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
