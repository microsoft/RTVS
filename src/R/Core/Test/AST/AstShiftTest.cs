using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.AST {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AstShiftTest : UnitTestBase {
        [TestMethod]
        [TestCategory("AST")]
        public void AstShiftTest1() {
            AstRoot ast = RParser.Parse(new TextStream(" a()"));
            IScope scope = ast.Children[0] as IScope;

            Assert.AreEqual(1, scope.Children[0].Start);
            ast.Shift(1);
            Assert.AreEqual(2, scope.Children[0].Start);
        }

        [TestMethod]
        [TestCategory("AST")]
        public void AstShiftTest2() {
            AstRoot ast = RParser.Parse(new TextStream(" a()"));
            IScope scope = ast.Children[0] as IScope;

            Assert.AreEqual(1, scope.Children[0].Start);

            IStatement statement = scope.Children[0] as IStatement;
            IExpression expression = statement.Children[0] as IExpression;
            Assert.AreEqual(1, expression.Children[0].Start);

            IFunction func = expression.Children[0] as IFunction;
            Assert.AreEqual(2, func.OpenBrace.Start);

            ast.ShiftStartingFrom(2, 1);
            Assert.AreEqual(3, func.OpenBrace.Start);
        }
    }
}
