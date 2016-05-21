// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Editor.Document.Definitions;

namespace Microsoft.R.Editor {
    public static class DocumentExtensions {
        public static bool IsPositionInComment(this IREditorDocument document, int position) {
            var ast = document?.EditorTree?.AstRoot;
            return ast != null && ast.Comments.GetItemContaining(position) >= 0;
        }
    }
}
