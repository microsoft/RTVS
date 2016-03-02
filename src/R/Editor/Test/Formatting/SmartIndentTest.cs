// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.SmartIndent]
    public class SmartIndentTest {
        [Test]
        public void SmartIndent_NoScopeTest01() {
            int? indent = GetSmartIndent("if (x > 1)\n", 1);

            indent.Should().HaveValue()
                .And.Be(4);
        }

        [Test]
        public void SmartIndent_UnclosedScopeTest01() {
            int? indent = GetSmartIndent("{if (x > 1)\r\n    x <- 1\r\nelse\n", 3);

            indent.Should().HaveValue()
                .And.Be(4);
        }

        [Test]
        public void SmartIndent_UnclosedScopeTest02() {
            int? indent = GetSmartIndent("repeat\r\n    if (x > 1)\r\n", 2);

            indent.Should().HaveValue()
                .And.Be(8);
        }

        [Test]
        public void SmartIndent_ScopedIfTest01() {
            int? indent = GetSmartIndent("if (x > 1) {\r\n\r\n}", 1);

            indent.Should().HaveValue()
                .And.Be(4);
        }

        private int? GetSmartIndent(string content, int lineNumber) {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(content, 0, out ast);
            var document = new EditorDocumentMock(new EditorTreeMock(textView.TextBuffer, ast));

            ISmartIndentProvider provider = EditorShell.Current.ExportProvider.GetExport<ISmartIndentProvider>().Value;
            ISmartIndent indenter = provider.CreateSmartIndent(textView);

            return indenter.GetDesiredIndentation(textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber));
        }
    }
}
