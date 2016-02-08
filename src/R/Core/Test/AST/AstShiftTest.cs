using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.AST {
    [ExcludeFromCodeCoverage]
    public class AstShiftTest {
        [Test]
        [Category.R.Ast]
        public void AstShiftTest1() {
            AstRoot ast = RParser.Parse(new TextStream(" a()"));
            IScope scope = ast.Children[0].Should().BeAssignableTo<IScope>().Which;

            scope.Children[0].Start.Should().Be(1);
            ast.Shift(1);
            scope.Children[0].Start.Should().Be(2);
        }

        [Test]
        [Category.R.Ast]
        public void AstShiftTest2() {
            AstRoot ast = RParser.Parse(new TextStream(" a()"));
            var scope = ast.Children[0].Should().BeAssignableTo<IScope>().Which;
            scope.Children[0].Start.Should().Be(1);

            var expression = scope.Children[0].Should().BeAssignableTo<IStatement>()
                .Which.Children[0].Should().BeAssignableTo<IExpression>()
                .Which;

            expression.Children[0].Start.Should().Be(1);
            var func = expression.Children[0].Should().BeAssignableTo<IFunction>()
                .Which;

            func.OpenBrace.Start.Should().Be(2);

            ast.ShiftStartingFrom(2, 1);

            func.OpenBrace.Start.Should().Be(3);
        }
    }
}
