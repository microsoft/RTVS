// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;

namespace Microsoft.R.Editor {
    public static class DocumentExtensions {
        public static bool IsPositionInComment(this IREditorDocument document, int position) {
            bool inComment = false;
            var editorTree = document?.EditorTree;
            if(editorTree != null) {
                var ast = editorTree.AstRoot;
                inComment = ast.Comments.GetItemsContainingInclusiveEnd(position).Count > 0;
                if(!inComment) {
                    var line = document.EditorBuffer.CurrentSnapshot.GetLineFromPosition(position);
                    position -= line.Start;
                    var tokens = (new RTokenizer()).Tokenize(line.GetText());
                    var token = tokens.FirstOrDefault(t => t.Contains(position) || t.End == position);
                    inComment = token != null && token.TokenType == RTokenType.Comment;
                }
            }
            return inComment;
        }
    }
}
