// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.PlatformUI;
using Xunit;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class TreeChangeTypeTest {
        private readonly IServiceContainer _services;

        public TreeChangeTypeTest(IServiceContainer services) {
            _services = services;
        }

        [CompositeTest]
        [InlineData(0, 0, 1, " ", TextChangeType.Trivial)]
        [InlineData(1, 1, 0, "", TextChangeType.Trivial)]
        [InlineData(1, 0, 1, "\n", TextChangeType.Trivial)]
        public void EditWhitespace(int start, int oldLength, int newLength, string newText, TextChangeType expected) {
            const string expression = "x <- a + b";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, start, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [CompositeTest]
        [InlineData(1, 0, "", TextChangeType.Trivial)]
        [InlineData(1, 2, "a", TextChangeType.Trivial)]
        [InlineData(1, 2, "\"", TextChangeType.Structure)]
        public void EditString(int oldLength, int newLength, string newText, TextChangeType expected) {
            const string expression = "x <- a + \"boo\"";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 10, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [Test]
        public void EditString04() {
            const string expression = "\"boo\"";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 1, 0, 1, "a")) {
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
        public void EditComment01(int oldLength, int newLength, string newText, TextChangeType expected) {
            const string expression = "x <- a + b # comment";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 12, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [Test]
        public void EditComment04() {
            const string expression = "# comment\n a <- b + c";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 9, 1, 0, string.Empty)) {
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Structure);
            }
        }

        [Test]
        public void EditComment05() {
            const string expression = "#";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 1, 0, 1, "a")) {
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Trivial);

                tree.AstRoot.Comments.Should().ContainSingle();
                var comment = tree.AstRoot.Comments[0];
                comment.Start.Should().Be(0);
                comment.Length.Should().Be(2);
            }
        }

        [Test]
        public void CurlyBrace() {
            const string expression = "if(true) {x <- 1} else ";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, expression.Length, 0, 1, "{")) {
                tree.IsDirty.Should().BeTrue();
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Structure);
            }
        }

        [CompositeTest]
        [InlineData(6, 0, 1, " ", TextChangeType.Structure)]
        public void AddWhitespace(int start, int oldLength, int newLength, string newText, TextChangeType expected) {
            const string expression = "x <- aa";

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, start, oldLength, newLength, newText)) {
                tree.PendingChanges.TextChangeType.Should().Be(expected);
            }
        }

        [CompositeTest]
        [InlineData("if(true) { } else { }", 12, 0, 1, "\n")]
        [InlineData("if(true) { } else { }", 12, 0, 2, "\r\n")]
        [InlineData("if(true) { }\nelse { }", 12, 1, 0, null)]
        [InlineData("      if(true) { }\nelse { }", 18, 1, 0, null)]
        public void ElsePosition(string content, int start, int oldLength, int newLength, string text) {
            using (var tree = EditorTreeTest.ApplyTextChange(_services, content, start, oldLength, newLength, text)) {
                tree.IsDirty.Should().BeTrue();
                tree.PendingChanges.TextChangeType.Should().Be(TextChangeType.Structure);
            }
        }
    }
}
