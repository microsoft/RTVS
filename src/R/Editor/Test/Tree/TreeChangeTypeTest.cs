// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class TreeChangeTypeTest {
        private readonly IExportProvider _exportProvider;
        private readonly ICoreShell _coreShell;

        public TreeChangeTypeTest(IExportProvider exportProvider, EditorTestFilesFixture testFiles) {
            _exportProvider = exportProvider;
            _coreShell = _exportProvider.GetExportedValue<ICoreShell>();
        }

        [CompositeTest]
        [InlineData(0, 0, 1, " ", TextChangeType.Trivial)]
        [InlineData(1, 1, 0, "", TextChangeType.Trivial)]
        [InlineData(1, 0, 1, "\n", TextChangeType.Trivial)]
        public void TextChange_EditWhitespaceTest(int start, int oldLength, int newLength, string newText, TextChangeType expected) {
            string expression = "x <- a + b";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, start, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [CompositeTest]
        [InlineData(1, 0, "", TextChangeType.Trivial)]
        [InlineData(1, 2, "a", TextChangeType.Trivial)]
        [InlineData(1, 2, "\"", TextChangeType.Structure)]
        public void TextChange_EditString(int oldLength, int newLength, string newText, TextChangeType expected) {
            string expression = "x <- a + \"boo\"";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, 10, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [Test]
        public void TextChange_EditString04() {
            string expression = "\"boo\"";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, 1, 0, 1, "a")) {
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Trivial);

                var token = tree.AstRoot.Children.Should().ContainSingle()
                    .Which.Should().BeAssignableTo<IScope>()
                    .Which.Children.Should().ContainSingle()
                    .Which.Should().BeAssignableTo<IStatement>()
                    .Which.Children.Should().ContainSingle()
                    .Which.Should().BeAssignableTo<IExpression>()
                    .Which.Children.Should().ContainSingle()
                    .Which.Should().BeAssignableTo<TokenNode>()
                    .Which.Token;

                token.TokenType.Should().Be(RTokenType.String);
                token.Start.Should().Be(0);
                token.Length.Should().Be(6);
            }
        }

        [CompositeTest]
        [InlineData(1, 0, "", TextChangeType.Trivial)]
        [InlineData(1, 1, "a", TextChangeType.Trivial)]
        [InlineData(1, 2, "\n", TextChangeType.Structure)]
        public void TextChange_EditComment01(int oldLength, int newLength, string newText, TextChangeType expected) {
            string expression = "x <- a + b # comment";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, 12, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [Test]
        public void TextChange_EditComment04() {
            string expression = "# comment\n a <- b + c";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, 9, 1, 0, string.Empty)) {
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Structure);
            }
        }

        [Test]
        public void TextChange_EditComment05() {
            string expression = "#";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, 1, 0, 1, "a")) {
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Trivial);

                tree.AstRoot.Comments.Should().ContainSingle();
                var comment = tree.AstRoot.Comments[0];
                comment.Start.Should().Be(0);
                comment.Length.Should().Be(2);
            }
        }

        [Test]
        public void TextChange_CurlyBrace() {
            string expression = "if(true) {x <- 1} else ";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, expression.Length, 0, 1, "{")) {
                tree.IsDirty.Should().BeTrue();
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Structure);
            }
        }

        [CompositeTest]
        [InlineData(6, 0, 1, " ", TextChangeType.Structure)]
        public void TextChange_AddWhitespace(int start, int oldLength, int newLength, string newText, TextChangeType expected) {
            string expression = "x <- aa";

            using (var tree = EditorTreeTest.ApplyTextChange(_coreShell, expression, start, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }
    }
}
