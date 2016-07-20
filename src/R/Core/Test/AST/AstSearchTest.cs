// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.AST {
    [ExcludeFromCodeCoverage]
    public class AstSearchTest {
        [Test]
        [Category.R.Ast]
        public void GetPackageNamesTest() {
            AstRoot ast = RParser.Parse(new TextStream("library(a); library(b); while(T) { library(c) }"));
            string[] names = ast.GetFilePackageNames().ToArray();

            names.Should().Equal("a", "b", "c");
        }

        [Test]
        [Category.R.Ast]
        public void GetPositionNodeTest() {
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
        public void GetElementsEnclosingRangeTest() {
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
        public void NodeFromRangeTest() {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a123+b"));

            IAstNode node = ast.NodeFromRange(new TextRange(2, 5));
            node.Should().BeAssignableTo<IOperator>();

            node = ast.NodeFromRange(new TextRange(7, 2));
            node.Should().BeOfType<Variable>();
        }
    }
}
