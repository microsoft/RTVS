// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    public static class EditorTreeTest {
        public static EditorTree MakeTree(IServiceContainer services, string expression) {
            var textBuffer = new TextBufferMock(expression, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var tree = new EditorTree(textBuffer, services);
            tree.Build();
            return tree;
        }

        public static EditorTree ApplyTextChange(IServiceContainer services, string expression, int start, int oldLength, int newLength, string newText) {
            var textBuffer = new TextBufferMock(expression, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var tree = new EditorTree(textBuffer, services);
            tree.Build();

            if (oldLength == 0 && newText.Length > 0) {
                textBuffer.Insert(start, newText);
            } else if (oldLength > 0 && !string.IsNullOrEmpty(newText)) {
                textBuffer.Replace(new TextRange(start, oldLength), newText);
            } else {
                textBuffer.Delete(new TextRange(start, oldLength));
            }

            return tree;
        }
    }
}
