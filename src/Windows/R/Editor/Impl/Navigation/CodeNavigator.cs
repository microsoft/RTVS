// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Navigation {
    internal static class CodeNavigator {
        public static SnapshotPoint? FindCurrentItemDefinition(ITextView textView, ITextBuffer textBuffer, out string itemName) {
            Span span;
            itemName = textView.GetIdentifierUnderCaret(out span);
            if (!string.IsNullOrEmpty(itemName)) {
                var position = textView.GetCaretPosition(textBuffer);
                if (position.HasValue) {
                    var document = textBuffer.GetEditorDocument<IREditorDocument>();
                    var itemDefinition = document.EditorTree.AstRoot.FindItemDefinition(position.Value, itemName);
                    if (itemDefinition != null) {
                        return textView.MapUpToView(document.EditorTree.TextSnapshot(), itemDefinition.Start);
                    }
                }
            }
            return null;
        }
    }
}
