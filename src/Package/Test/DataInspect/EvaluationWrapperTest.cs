using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Languages.Editor.Shell;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class EvaluationWrapperTest {

        public EvaluationWrapperTest() {
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task GlobalEnvironmentTest() {
            using (var hostScript = new VariableRHostScript()) {
                await hostScript.EvaluateAsync("ls()"); // run anything

                var target = hostScript.GlobalEnvironment;
                target.Name.Should().BeEquivalentTo("Global Environment");
            }
        }

        // TODO: RStudio difference
        //    value.integer.1   RS 1L                    RTVS just 1
        //    value.numeric.big RS 98765432109876543210  RTVS 9.88e+19
        //    value.date        RS 2015-08-01            RTVS Date, format: "2015-08-01"
        // NOTE: value.date  HasChildren is true. really?
        object[,] valueTestData = new object[,] {
            { "value.na <- NA", new VariableExpectation() { Name = "value.na", Value = "NA", TypeName = "logical", Class = "logical", HasChildren = false, CanShowDetail = false } },
            { "value.null <- NULL", new VariableExpectation() { Name = "value.null", Value = "NULL", TypeName = "NULL", Class = "NULL", HasChildren = false, CanShowDetail = false } },
            { "value.NaN <- NaN", new VariableExpectation() { Name = "value.NaN", Value = "NaN", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.character <- 'abcdefghijklmnopqrstuvwxyz'", new VariableExpectation() { Name = "value.character", Value = "\"abcdefghijklmnopqrstuvwxyz\"", TypeName = "character", Class = "character", HasChildren = false, CanShowDetail = false } },
            { "value.character.1 <- 'abcde\"fhi,kjl \"op,qr\" s @t#u$v%w^x&y*z*()./\\`-+_=!'", new VariableExpectation() { Name = "value.character.1", Value = "\"abcde\\\"fhi,kjl \\\"op,qr\\\" s @t#u$v%w^x&y*z*()./`-+_=!\"", TypeName = "character", Class = "character", HasChildren = false, CanShowDetail = false } },
            { "value.numeric.1 <- 1", new VariableExpectation() { Name = "value.numeric.1", Value = "1", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.numeric.negative <- -123456", new VariableExpectation() { Name = "value.numeric.negative", Value = "-123456", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.numeric.big <- 98765432109876543210.9876543210", new VariableExpectation() { Name = "value.numeric.big", Value = "9.88e+19", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.integer.1 <- 1L", new VariableExpectation() { Name = "value.integer.1", Value = "1", TypeName = "integer", Class = "integer", HasChildren = false, CanShowDetail = false } },
            { "value.integer.negative <- -123456L", new VariableExpectation() { Name = "value.integer.negative", Value = "-123456", TypeName = "integer", Class = "integer", HasChildren = false, CanShowDetail = false } },
            { "value.complex <- complex(real=100, imaginary=100)", new VariableExpectation() { Name = "value.complex", Value = "100+100i", TypeName = "complex", Class = "complex", HasChildren = false, CanShowDetail = false } },
            { "value.complex.neg <- complex(real=-200, imaginary=-900)", new VariableExpectation() { Name = "value.complex.neg", Value = "-200-900i", TypeName = "complex", Class = "complex", HasChildren = false, CanShowDetail = false } },
            { "value.logical <- TRUE", new VariableExpectation() { Name = "value.logical", Value = "TRUE", TypeName = "logical", Class = "logical", HasChildren = false, CanShowDetail = false } },
            { "value.date <- as.Date('2015-08-01')", new VariableExpectation() { Name = "value.date", Value = "Date, format: \"2015-08-01\"", TypeName = "double", Class = "Date", HasChildren = true, CanShowDetail = false } },
        };

        object[,] factorTestData = new object[,] {
            { "factor.5 <- factor(1:5)", new VariableExpectation() { Name = "factor.5", Value = "Factor w/ 5 levels \"1\",\"2\",\"3\",\"4\",..: 1 2 3 4 5", TypeName = "integer", Class = "factor", HasChildren = true, CanShowDetail = false } },
            { "factor.ordered <- ordered(c('5','4','100','2','1'))", new VariableExpectation() { Name = "factor.ordered", Value = "Ord.factor w/ 5 levels \"1\"<\"100\"<\"2\"<..: 5 4 2 3 1", TypeName = "integer", Class = "ordered, factor", HasChildren = true, CanShowDetail = false } },
            { "factor.gender <- factor(c('male','female','male','male','female'))", new VariableExpectation() { Name = "factor.gender", Value = "Factor w/ 2 levels \"female\",\"male\": 2 1 2 2 1", TypeName = "integer", Class = "factor", HasChildren = true, CanShowDetail = false } },
        };

        object[,] formulaTestData = new object[,] {
            { "class(fo <- y~x1 * x2)", new VariableExpectation() { Name = "fo", Value = "Class 'formula' length 3 y ~ x1 * x2", TypeName = "language", Class = "formula", HasChildren = true, CanShowDetail = false } },
        };

        object[,] expressionTestData = new object[,] {
            { "expr <- expression('print(\"hello\")', '1+2', 'print(\"world\")', 'ls()')", new VariableExpectation() { Name = "expr", Value = "expression(\"print(\\\"hello\\\")\", \"1+2\", \"print(\\\"world\\\")\") ...", TypeName = "expression", Class = "expression", HasChildren = true, CanShowDetail = false } },
        };

        object[,] listTestData = new object[,] {
            { "list.length1 <- list(c(1, 2, 3))", new VariableExpectation() { Name = "list.length1", Value = "List of 1", TypeName = "list", Class = "list", HasChildren = true, CanShowDetail = false } },
        };

        [Test]
        [Category.Variable.Explorer]
        public Task ValuesTest() {
            return RunTest(valueTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task FactorTest() {
            return RunTest(factorTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task FormulaTest() {
            return RunTest(formulaTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task ExpressionTest() {
            return RunTest(expressionTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task ListTest() {
            return RunTest(listTestData);
        }

        private static async Task RunTest(object[,] testData) {
            using (var hostScript = new VariableRHostScript()) {
                int testCount = testData.GetLength(0);

                for (int i = 0; i < testCount; i++) {
                    await hostScript.EvaluateAndAssert(
                        (string)testData[i, 0],
                        (VariableExpectation)testData[i, 1]);
                }
            }
        }
    }
}
