// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.AST {
    [ExcludeFromCodeCoverage]
    public class AstNodeTest {
        [Test]
        [Category.R.Ast]
        public void AstNode_RemoveChildrenTest() {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a+b"));

            IOperator op = ast.Children[0].Should().BeAssignableTo<IScope>()
                .Which.Children[0].Should().BeAssignableTo<IStatement>()
                .Which.Children[0].Should().BeAssignableTo<IExpression>()
                .Which.Children[0].Should().BeAssignableTo<IOperator>()
                .Which;

            op.Children.Should().HaveCount(3);

            op.RemoveChildren(1, 0);
            op.Children.Should().HaveCount(3);

            op.RemoveChildren(0, 1);
            op.Children.Should().HaveCount(2);

            op.RemoveChildren(0, 0);
            op.Children.Should().HaveCount(2);

            op.RemoveChildren(1, 1);
            op.Children.Should().HaveCount(1);
        }

        [Test]
        [Category.R.Ast]
        public void AstNode_GetPositionNodeTest() {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a+b"));

            IAstNode scope;
            IAstNode variable;

            ast.GetPositionNode(0, out scope);
            scope.Should().BeAssignableTo<IScope>();

            scope.GetPositionNode(1, out variable);
            variable.Should().BeOfType<Variable>();
        }

        [Test]
        [Category.R.Ast]
        public void AstNode_GetElementsEnclosingRangeTest() {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a123+b"));

            IAstNode startNode, endNode;
            PositionType startPositionType, endPositionType;

            ast.GetElementsEnclosingRange(2, 5, out startNode, out startPositionType, out endNode, out endPositionType);

            startNode.Should().BeAssignableTo<IOperator>();
            endNode.Should().BeOfType<Variable>();

            startPositionType.Should().Be(PositionType.Node);
            endPositionType.Should().Be(PositionType.Token);

            ((Variable) endNode).Name.Should().Be("a123");
        }

        [Test]
        [Category.R.Ast]
        public void AstNode_NodeFromRangeTest() {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a123+b"));

            IAstNode node = ast.NodeFromRange(new TextRange(2, 5));
            node.Should().BeAssignableTo<IOperator>();

            node = ast.NodeFromRange(new TextRange(7, 2));
            node.Should().BeOfType<Variable>();
        }

        [Test]
        [Category.R.Ast]
        public void AstNode_PropertiesTest() {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a+b"));

            ast.Properties.AddProperty("a", "b");

            ast.Properties.PropertyList.Should().HaveCount(1);
            ast.Properties.ContainsProperty("a").Should().BeTrue();
            ast.Properties.ContainsProperty("b").Should().BeFalse();
            ast.Properties.GetProperty("a").Should().Be("b");
            ast.Properties.GetProperty<object>("a").Should().Be("b");

            ast.Properties["a"] = "c";
            ast.Properties.ContainsProperty("a").Should().BeTrue();
            ast.Properties.ContainsProperty("b").Should().BeFalse();
            ast.Properties.GetProperty("a").Should().Be("c");
            ast.Properties.GetProperty<object>("a").Should().Be("c");

            string s;
            ast.Properties.TryGetProperty("a", out s).Should().BeTrue();
            s.Should().Be("c");

            ast.Properties.RemoveProperty("a");
            ast.Properties.PropertyList.Should().BeEmpty();
            ast.Properties.ContainsProperty("a").Should().BeFalse();
        }
    }
}
