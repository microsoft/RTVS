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
        public void SimpleScopeTest01() {
            int? indent = GetSmartIndent("if (x > 1)\n", 1);
            indent.Should().HaveValue().And.Be(4);
        }

        [Test]
        public void SimpleScopeTest02() {
            int? indent = GetSmartIndent("{if (x > 1)\r\n    x <- 1\r\nelse\n", 3);
            indent.Should().HaveValue().And.Be(4);
        }

        [Test]
        public void SimpleScopeTest03() {
            int? indent = GetSmartIndent("repeat\r\n    if (x > 1)\r\n", 2);
            indent.Should().HaveValue().And.Be(8);
        }

        [Test]
        public void SimpleScopeTest04() {
            string content = "if (TRUE)\n    x <- 1\n\n";
            int? indent = GetSmartIndent(content, 3);
            indent.Should().HaveValue().And.Be(0);
        }

        [Test]
        public void FunctionArguments01() {
            string content = "func(a,\n";
            int? indent = GetSmartIndent(content, 1);
            indent.Should().HaveValue().And.Be(5);
        }

        [Test]
        public void FunctionDefinitionArguments01() {
            string content = "x <- function(a,\n";
            int? indent = GetSmartIndent(content, 1);
            indent.Should().HaveValue().And.Be(14);
        }

        [Test]
        public void ScopedIfTest01() {
            int? indent = GetSmartIndent("if (x > 1) {\r\n\r\n}", 1);
            indent.Should().HaveValue().And.Be(4);
        }

        [Test]
        public void ClosingBrace01() {
            int? indent = GetSmartIndent("if (x > 1) {\n\n}", 2);
            indent.Should().HaveValue().And.Be(0);
        }

        [Test]
        public void ClosingBrace02() {
            int? indent = GetSmartIndent("while (TRUE) {\n    if (x > 1) {\n\n    }\n}", 4);
            indent.Should().HaveValue().And.Be(0);
        }

        [Test]
        public void AfterFunction01() {
            int? indent = GetSmartIndent("library(abind)\n", 1);
            indent.Should().HaveValue().And.Be(0);
        }

        [Test]
        public void FunctionDefinitionScope01() {
            int? indent = GetSmartIndent("x <- f(a) {\n", 1);
            indent.Should().HaveValue().And.Be(4);
        }

        [Test]
        public void FunctionDefinitionScope02() {
            int? indent = GetSmartIndent("x <- f(a,\nb) {\n", 2);
            indent.Should().HaveValue().And.Be(4);
        }

        [Test]
        public void FunctionDefinitionScope03() {
            int? indent = GetSmartIndent("x <- f(a) {\n\n}", 1);
            indent.Should().HaveValue().And.Be(4);
        }

        [Test]
        public void UnclosedScope02() {
            int? indent = GetSmartIndent("if(1) {\n", 1);
            indent.Should().HaveValue().And.Be(4);
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
