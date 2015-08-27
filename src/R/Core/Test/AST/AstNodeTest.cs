using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.AST
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AstNodeTest : UnitTestBase
    {
        [TestMethod]
        public void AstNode_RemoveChildrenTest()
        {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a+b"));
            IScope scope = ast.Children[0] as IScope;
            IStatement statement = scope.Children[0] as IStatement;
            IExpression exp = statement.Children[0] as IExpression;

            IOperator op = exp.Children[0] as IOperator;
            Assert.AreEqual(3, op.Children.Count);

            op.RemoveChildren(1, 0);
            Assert.AreEqual(3, op.Children.Count);

            op.RemoveChildren(0, 1);
            Assert.AreEqual(2, op.Children.Count);

            op.RemoveChildren(0, 0);
            Assert.AreEqual(2, op.Children.Count);

            op.RemoveChildren(1, 1);
            Assert.AreEqual(1, op.Children.Count);
        }

        [TestMethod]
        public void AstNode_GetPositionNodeTest()
        {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a+b"));

            IAstNode scope;
            IAstNode variable;

            ast.GetPositionNode(0, out scope);
            Assert.IsNotNull(scope);
            Assert.IsTrue(scope is IScope);

            scope.GetPositionNode(1, out variable);
            Assert.IsNotNull(variable);
            Assert.IsTrue(variable is Variable);
        }

        [TestMethod]
        public void AstNode_GetElementsEnclosingRangeTest()
        {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a123+b"));

            IAstNode startNode, endNode;
            PositionType startPositionType, endPositionType;

            ast.GetElementsEnclosingRange(2, 5, out startNode, out startPositionType, out endNode, out endPositionType);

            Assert.IsNotNull(startNode);
            Assert.IsNotNull(endNode);

            Assert.AreEqual(PositionType.Node, startPositionType);
            Assert.IsTrue(startNode is IOperator);

            Assert.AreEqual(PositionType.Node, endPositionType);
            Assert.IsTrue(endNode is Variable);

            Assert.AreEqual("a123", ((Variable)endNode).Name);
        }

        [TestMethod]
        public void AstNode_NodeFromRangeTest()
        {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a123+b"));

            IAstNode node = ast.NodeFromRange(new TextRange(2, 5));
            Assert.IsNotNull(node);
            Assert.IsTrue(node is IOperator);

            node = ast.NodeFromRange(new TextRange(7, 2));
            Assert.IsNotNull(node);
            Assert.IsTrue(node is Variable);
        }

        [TestMethod]
        public void AstNode_PropertiesTest()
        {
            AstRoot ast = RParser.Parse(new TextStream(" x <- a+b"));

            ast.Properties.AddProperty("a", "b");
            Assert.AreEqual(1, ast.Properties.PropertyList.Count);
            Assert.IsTrue(ast.Properties.ContainsProperty("a"));
            Assert.IsFalse(ast.Properties.ContainsProperty("b"));
            Assert.AreEqual("b", ast.Properties.GetProperty("a"));
            Assert.AreEqual("b", ast.Properties.GetProperty<object>("a"));

            ast.Properties["a"] = "c";
            Assert.IsTrue(ast.Properties.ContainsProperty("a"));
            Assert.IsFalse(ast.Properties.ContainsProperty("b"));
            Assert.AreEqual("c", ast.Properties.GetProperty("a"));
            Assert.AreEqual("c", ast.Properties.GetProperty<object>("a"));

            string s;
            Assert.IsTrue(ast.Properties.TryGetProperty("a", out s));
            Assert.AreEqual("c", s);

            ast.Properties.RemoveProperty("a");
            Assert.AreEqual(0, ast.Properties.PropertyList.Count);
            Assert.IsFalse(ast.Properties.ContainsProperty("a"));
        }
    }
}
