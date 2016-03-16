// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Tree.Definitions;

namespace Microsoft.R.Editor.Tree {
    public static class TreeExtensions {
        public static AstRoot GetCurrentRootOrPreviousIfNotReady(this IEditorTree tree) {
            return (!tree.IsReady && tree.PreviousAstRoot != null) ? tree.PreviousAstRoot : tree.AstRoot;
        }
    }
}
