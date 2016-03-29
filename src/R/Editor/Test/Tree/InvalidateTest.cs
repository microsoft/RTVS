// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Statements.Conditionals;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class EditorTreeInvalidateTest {
        [Test]
        public void InvalidateAll() {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b");
            tree.Invalidate();
            tree.AstRoot.Children.Should().ContainSingle();
            tree.AstRoot.Children[0].Children.Should().BeEmpty();
        }

        [Test]
        public void InvalidateInRangeTest01() {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b");

            bool nodesChanged = tree.InvalidateInRange(new TextRange(4, 1));
            nodesChanged.Should().BeTrue();

            tree.AstRoot.Children.Should().ContainSingle();
            tree.AstRoot.Children[0].Should().BeOfType<GlobalScope>();
            tree.AstRoot.Children[0].Children.Should().BeEmpty();
        }

        [Test]
        public void InvalidateInRangeTest02() {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) { x <- a + b }");

            bool nodesChanged = tree.InvalidateInRange(new TextRange(11, 10));
            nodesChanged.Should().BeTrue();

            tree.AstRoot.Children.Should().ContainSingle();
            tree.AstRoot.Children[0].Should().BeOfType<GlobalScope>();

            tree.AstRoot.Children[0].Children.Should().NotBeEmpty();
            tree.AstRoot.Children[0].Children[0].Should().BeOfType<If>();

            var ifStatement = tree.AstRoot.Children[0].Children[0] as If;
            ifStatement.Should().NotBeNull();
            ifStatement.Scope.Should().NotBeNull();
            ifStatement.Scope.Children.Count.Should().Be(2);
            ifStatement.Scope.OpenCurlyBrace.Should().NotBeNull();
            ifStatement.Scope.CloseCurlyBrace.Should().NotBeNull();
        }

        [Test]
        public void InvalidateInRangeTest03() {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b ");

            bool nodesChanged = tree.InvalidateInRange(new TextRange(9, 8));
            nodesChanged.Should().BeTrue();

            tree.AstRoot.Children.Should().ContainSingle();
            tree.AstRoot.Children[0].Should().BeOfType<GlobalScope>();
            tree.AstRoot.Children[0].Children.Should().BeEmpty();
        }
    }
}
