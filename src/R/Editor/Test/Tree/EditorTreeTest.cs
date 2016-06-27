// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    public static class EditorTreeTest {
        public static EditorTree MakeTree(ICoreShell coreShell, string expression) {
            TextBufferMock textBuffer = new TextBufferMock(expression, RContentTypeDefinition.ContentType);

            var tree = new EditorTree(textBuffer, coreShell);
            tree.Build();

            return tree;
        }

        public static EditorTree ApplyTextChange(ICoreShell coreShell, string expression, int start, int oldLength, int newLength, string newText) {
            TextBufferMock textBuffer = new TextBufferMock(expression, RContentTypeDefinition.ContentType);
            var tree = new EditorTree(textBuffer, coreShell);
            tree.Build();

            TextChange tc = new TextChange();
            tc.OldRange = new TextRange(start, oldLength);
            tc.NewRange = new TextRange(start, newLength);
            tc.OldTextProvider = new TextProvider(textBuffer.CurrentSnapshot);

            if (oldLength == 0 && newText.Length > 0) {
                textBuffer.Insert(start, newText);
            } else if (oldLength > 0 && newText.Length > 0) {
                textBuffer.Replace(new Span(start, oldLength), newText);
            } else {
                textBuffer.Delete(new Span(start, oldLength));
            }

            return tree;
        }
    }
}
