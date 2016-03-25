// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Navigation {
    internal static class CodeNavigator {
        public static SnapshotPoint? FindCurrentItemDefinition(ITextView textView, ITextBuffer textBuffer) {
            Span span;
            string itemName = textView.GetIdentifierUnderCaret(out span);
            if (!string.IsNullOrEmpty(itemName)) {
                 var position = REditorDocument.MapCaretPositionFromView(textView);
                if (position.HasValue) {
                    var itemDefinition = FindItemDefinition(textBuffer, position.Value, itemName);
                    if (itemDefinition != null) {
                        return textView.MapUpToBuffer(itemDefinition.Start, textView.TextBuffer);
                    }
                }
            }
            return null;
        }

        public static IAstNode FindItemDefinition(ITextBuffer textBuffer, int position, string itemName) {
            var document = REditorDocument.FromTextBuffer(textBuffer);
            var ast = document.EditorTree.AstRoot;
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            var func = scope.FindFunctionByName(itemName, position);
            if (func != null) {
                return func.Value;
            } else {
                var v = scope.FindVariableByName(itemName, position);
                if (v != null) {
                    return v;
                }
            }
            return null;
        }
    }
}
