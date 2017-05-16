// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Category.Variable.Explorer]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public sealed class EvaluationWrapperTest : HostBasedInteractiveTest {
        private readonly VariableRHostScript _hostScript;

        public EvaluationWrapperTest(IServiceContainer services): base(new VariableRHostScript(services), services) {
            _hostScript = GetScript<VariableRHostScript>();
        }


        // TODO: RStudio difference
        //    value.integer.1   RS 1L                    RTVS just 1
        //    value.numeric.big RS 98765432109876543210  RTVS 9.88e+19
        //    value.date        RS 2015-08-01            RTVS Date, format: "2015-08-01"
        // NOTE: value.date  HasChildren is true. really?
        object[,] valueTestData = new object[,] {

        };

        object[,] factorTestData = new object[,] {
            { "factor.5 <- factor(1:5)", new VariableExpectation() { Name = "factor.5", Value = "Factor w/ 5 levels \"1\",\"2\",\"3\",\"4\",..: 1 2 3 4 5", TypeName = "integer", Class = "factor", HasChildren = true, CanShowDetail = true } },
            { "factor.ordered <- ordered(c('5','4','100','2','1'))", new VariableExpectation() { Name = "factor.ordered", Value = "Ord.factor w/ 5 levels \"1\"<\"100\"<\"2\"<..: 5 4 2 3 1", TypeName = "integer", Class = "ordered, factor", HasChildren = true, CanShowDetail = true } },
            { "factor.gender <- factor(c('male','female','male','male','female'))", new VariableExpectation() { Name = "factor.gender", Value = "Factor w/ 2 levels \"female\",\"male\": 2 1 2 2 1", TypeName = "integer", Class = "factor", HasChildren = true, CanShowDetail = true } },
        };

        object[,] formulaTestData32 = new object[,] {
            { "class(fo <- y~x1 * x2)", new VariableExpectation() { Name = "fo", Value = "Class 'formula' length 3 y ~ x1 * x2", TypeName = "language", Class = "formula", HasChildren = true, CanShowDetail = true } },
        };

        object[,] formulaTestData33 = new object[,] {
            { "class(fo <- y~x1 * x2)", new VariableExpectation() { Name = "fo", Value = "Class 'formula'  language y ~ x1 * x2", TypeName = "language", Class = "formula", HasChildren = true, CanShowDetail = true } },
        };

        object[,] expressionTestData = new object[,] {
            { "expr <- expression('print(\"hello\")', '1+2', 'print(\"world\")', 'ls()')", new VariableExpectation() { Name = "expr", Value = "expression(\"print(\\\"hello\\\")\", \"1+2\", \"print(\\\"world\\\")\") ...", TypeName = "expression", Class = "expression", HasChildren = true, CanShowDetail = false } },
        };

        object[,] listTestData = new object[,] {
            { "list.length1 <- list(c(1, 2, 3))", new VariableExpectation() { Name = "list.length1", Value = "List of 1", TypeName = "list", Class = "list", HasChildren = true, CanShowDetail = false } },
            { "list.length3 <- list(1, 2, 3)", new VariableExpectation() { Name = "list.length3", Value = "List of 3", TypeName = "list", Class = "list", HasChildren = true, CanShowDetail = true } },
        };

        object[,] activeBindingTestData = new object[,] {
            { "makeActiveBinding('z.activebinding1', function() 123, .GlobalEnv);", new VariableExpectation() { Name = "z.activebinding1", Value = "<active binding>", TypeName = "<active binding>", Class = "<active binding>", HasChildren = false, CanShowDetail = false } },
        };

        [CompositeTest]
        [InlineData("value.na <- NA", "value.na", "NA", "logical", "logical", false, false)]
        [InlineData("value.null <- NULL", "value.null", "NULL", "NULL", "NULL", false, false)]
        [InlineData("value.NaN <- NaN", "value.NaN", "NaN", "double", "numeric", false, false)]
        [InlineData("value.character <- 'abcdefghijklmnopqrstuvwxyz'", "value.character", "\"abcdefghijklmnopqrstuvwxyz\"", "character", "character", false, false)]
        [InlineData("value.character.1 <- 'abcde\"fhi,kjl \"op,qr\" s @t#u$v%w^x&y*z*()./\\`-+_=!'", "value.character.1", "\"abcde\\\"fhi,kjl \\\"op,qr\\\" s @t#u$v%w^x&y*z*()./`-+_=!\"", "character", "character", false, false)]
        [InlineData("value.numeric.1 <- 1", "value.numeric.1", "1", "double", "numeric", false, false)]
        [InlineData("value.numeric.negative <- -123456", "value.numeric.negative", "-123456", "double", "numeric", false, false)]
        [InlineData("value.numeric.big <- 98765432109876543210.9876543210", "value.numeric.big", "9.88e+19", "double", "numeric", false, false)]
        [InlineData("value.integer.1 <- 1L", "value.integer.1", "1", "integer", "integer", false, false)]
        [InlineData("value.integer.negative <- -123456L", "value.integer.negative", "-123456", "integer", "integer", false, false)]
        [InlineData("value.complex <- complex(real=100, imaginary=100)", "value.complex", "100+100i", "complex", "complex", false, false)]
        [InlineData("value.complex.neg <- complex(real=-200, imaginary=-900)", "value.complex.neg", "-200-900i", "complex", "complex", false, false)]
        [InlineData("value.logical <- TRUE", "value.logical", "TRUE", "logical", "logical", false, false)]
        [InlineData("value.date <- as.Date('2015-08-01')", "value.date", "Date, format: \"2015-08-01\"", "double", "Date", true, false)]
        [Category.Variable.Explorer]
        public async Task ValuesTest(string script, string expectedName, string expectedValue, string expectedTypeName, string expectedClass, bool expectedHasChildren, bool expectedCanShowDetail) {
            var expected = new VariableExpectation {
                Name = expectedName,
                Value = expectedValue,
                TypeName = expectedTypeName,
                Class = expectedClass,
                HasChildren = expectedHasChildren,
                CanShowDetail = expectedCanShowDetail
            };

            await _hostScript.EvaluateAndAssert(script, expected, VariableRHostScript.AssertEvaluationWrapper);
        }

        [Test]
        public Task FactorTest() {
            return RunTest(factorTestData);
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/2271")]
        public Task FormulaTest() {
            return RunTest(VariableRHostScript.RVersion < new Version(3, 3) ? formulaTestData32 : formulaTestData33);
        }

        [Test]
        public Task ExpressionTest() {
            return RunTest(expressionTestData);
        }

        [Test]
        public Task ListTest() {
            return RunTest(listTestData);
        }

        [Test]
        public Task ActiveBindingTest() {
            return RunTest(activeBindingTestData);
        }

        [Test]
        public async Task TruncateGrandChildrenTest() {
            await _hostScript.EvaluateAsync("x.truncate.children<-1:100");
            var children = await _hostScript.GlobalEnvrionment.GetChildrenAsync();
            var child = children.First(c => c.Name == "x.truncate.children");

            var grandChildren = await child.GetChildrenAsync();

            grandChildren.Count.Should().Be(21);   // truncate 20 + ellipsis
            grandChildren[20].Value.Should().Be(Resources.VariableExplorer_Truncated);
        }

        [Test]
        public async Task Matrix10x100Test() {
            var script = "matrix.10x100 <-matrix(1:1000, 10, 100)";
            var expectation = new VariableExpectation() {
                Name = "matrix.10x100",
                Value = "int [1:10, 1:100] 1 2 3 4 5 6 7 8 9 10 ...",
                TypeName = "integer",
                Class = "matrix",
                HasChildren = true,
                CanShowDetail = true
            };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 2);
            var columnRange = new Range(1, 3);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[1].Should().Be("[,2]");
            grid.ColumnHeader[2].Should().Be("[,3]");
            grid.ColumnHeader[3].Should().Be("[,4]");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("[1,]");
            grid.RowHeader[1].Should().Be("[2,]");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 1].Should().Be("11");
            grid.Grid[0, 2].Should().Be("21");
            grid.Grid[0, 3].Should().Be("31");
            grid.Grid[1, 1].Should().Be("12");
            grid.Grid[1, 2].Should().Be("22");
            grid.Grid[1, 3].Should().Be("32");
        }

        [Test]
        public async Task MatrixNamedTest() {
            var script = "matrix.named <- matrix(1:10, 2, 5, dimnames = list(r = c('r1', 'r2'), c = c('a', 'b', 'c', 'd', 'e')))";
            var expectation = new VariableExpectation() { Name = "matrix.named", Value = "int [1:2, 1:5] 1 2 3 4 5 6 7 8 9 10", TypeName = "integer", Class = "matrix", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            Range rowRange = new Range(0, 2);
            Range columnRange = new Range(2, 3);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[2].Should().Be("c");
            grid.ColumnHeader[3].Should().Be("d");
            grid.ColumnHeader[4].Should().Be("e");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("r1");
            grid.RowHeader[1].Should().Be("r2");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 2].Should().Be("5");
            grid.Grid[0, 3].Should().Be("7");
            grid.Grid[0, 4].Should().Be("9");
            grid.Grid[1, 2].Should().Be("6");
            grid.Grid[1, 3].Should().Be("8");
            grid.Grid[1, 4].Should().Be("10");
        }

        [Test]
        public async Task MatrixNATest() {
            var script = "matrix.na.header <- matrix(c(1, 2, 3, 4, NA, NaN, 7, 8, 9, 10), 2, 5, dimnames = list(r = c('r1', NA), c = c('a', 'b', NA, 'd', NA)))";
            var expectation = new VariableExpectation() { Name = "matrix.na.header", Value = "num [1:2, 1:5] 1 2 3 4 NA NaN 7 8 9 10", TypeName = "double", Class = "matrix", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 2);
            var columnRange = new Range(2, 3);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[2].Should().Be("[,1]");
            grid.ColumnHeader[3].Should().Be("d");
            grid.ColumnHeader[4].Should().Be("[,3]");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("r1");
            grid.RowHeader[1].Should().Be("[2,]");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 2].Should().Be("NA");
            grid.Grid[0, 3].Should().Be("7");
            grid.Grid[0, 4].Should().Be("9");
            grid.Grid[1, 2].Should().Be("NaN");
            grid.Grid[1, 3].Should().Be("8");
            grid.Grid[1, 4].Should().Be("10");
        }

        [Test]
        public async Task MatrixOneRowColumnTest() {
            var script1 = "matrix.singlerow <- matrix(1:3, nrow=1);";
            var expectation1 = new VariableExpectation() {
                Name = "matrix.singlerow",
                Value = "int [1, 1:3] 1 2 3",
                TypeName = "integer",
                Class = "matrix",
                HasChildren = true,
                CanShowDetail = true
            };

            var script2 = "matrix.singlecolumn <- matrix(1:3, ncol=1);";
            var expectation2 = new VariableExpectation() {
                Name = "matrix.singlecolumn",
                Value = "int [1:3, 1] 1 2 3",
                TypeName = "integer",
                Class = "matrix",
                HasChildren = true,
                CanShowDetail = true
            };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script1,
                expectation1,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 1);
            var columnRange = new Range(0, 3);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[0].Should().Be("[,1]");
            grid.ColumnHeader[1].Should().Be("[,2]");
            grid.ColumnHeader[2].Should().Be("[,3]");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("[1,]");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 0].Should().Be("1");
            grid.Grid[0, 1].Should().Be("2");
            grid.Grid[0, 2].Should().Be("3");


            evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script2,
                expectation2,
                VariableRHostScript.AssertEvaluationWrapper);

            rowRange = new Range(0, 3);
            columnRange = new Range(0, 1);
            grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[0].Should().Be("[,1]");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("[1,]");
            grid.RowHeader[1].Should().Be("[2,]");
            grid.RowHeader[2].Should().Be("[3,]");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 0].Should().Be("1");
            grid.Grid[1, 0].Should().Be("2");
            grid.Grid[2, 0].Should().Be("3");
        }

        [Test]
        public async Task MatrixOnlyRowNameTest() {
            var script = "matrix.rowname.na <- matrix(c(1,2,3,4), nrow=2, ncol=2);rownames(matrix.rowname.na)<-c(NA, 'row2');";
            var expectation = new VariableExpectation() { Name = "matrix.rowname.na", Value = "num [1:2, 1:2] 1 2 3 4", TypeName = "double", Class = "matrix", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 2);
            var columnRange = new Range(0, 2);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[0].Should().Be("[,1]");
            grid.ColumnHeader[1].Should().Be("[,2]");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("[1,]");
            grid.RowHeader[1].Should().Be("row2");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 0].Should().Be("1");
            grid.Grid[0, 1].Should().Be("3");
            grid.Grid[1, 0].Should().Be("2");
            grid.Grid[1, 1].Should().Be("4");
        }

        [Test]
        public async Task MatrixOnlyColumnNameTest() {
            var script = "matrix.colname.na <- matrix(1:6, nrow=2, ncol=3);colnames(matrix.colname.na)<-c('col1',NA,'col3');";
            var expectation = new VariableExpectation() { Name = "matrix.colname.na", Value = "int [1:2, 1:3] 1 2 3 4 5 6", TypeName = "integer", Class = "matrix", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 2);
            var columnRange = new Range(0, 3);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[0].Should().Be("col1");
            grid.ColumnHeader[1].Should().Be("[,2]");
            grid.ColumnHeader[2].Should().Be("col3");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("[1,]");
            grid.RowHeader[1].Should().Be("[2,]");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 0].Should().Be("1");
            grid.Grid[0, 1].Should().Be("3");
            grid.Grid[0, 2].Should().Be("5");
            grid.Grid[1, 0].Should().Be("2");
            grid.Grid[1, 1].Should().Be("4");
            grid.Grid[1, 2].Should().Be("6");
        }

        [Test]
        public async Task MatrixLargeCellTest() {
            var script = "matrix.largecell <- matrix(list(as.double(1:5000), 2, 3, 4), nrow = 2, ncol = 2);";
            var expectation = new VariableExpectation() { Name = "matrix.largecell", Value = "List of 4", TypeName = "list", Class = "matrix", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 1);
            var columnRange = new Range(0, 1);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.RowHeader.Range.Should().Be(rowRange);
            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));

            grid.Grid[0, 0].Should().Be("1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27...");
        }

        [Test]
        public async Task DataFrameTest() {
            var script = "df.test <- data.frame(101:103, c('\"a', 'b', 'c'))";
            var expectation = new VariableExpectation() { Name = "df.test", Value = "3 obs. of  2 variables", TypeName = "list", Class = "data.frame", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 3);
            var columnRange = new Range(0, 2);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[0].Should().Be("X101.103");
            grid.ColumnHeader[1].Should().Be("c....a....b....c..");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("1");
            grid.RowHeader[1].Should().Be("2");
            grid.RowHeader[2].Should().Be("3");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 0].Should().Be("101");
            grid.Grid[0, 1].Should().Be("\"a");
            grid.Grid[1, 0].Should().Be("102");
            grid.Grid[1, 1].Should().Be("b");
            grid.Grid[2, 0].Should().Be("103");
            grid.Grid[2, 1].Should().Be("c");

            // single column
            columnRange = new Range(1, 1);
            grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));
            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[1].Should().Be("c....a....b....c..");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("1");
            grid.RowHeader[1].Should().Be("2");
            grid.RowHeader[2].Should().Be("3");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 1].Should().Be("\"a");
            grid.Grid[1, 1].Should().Be("b");
            grid.Grid[2, 1].Should().Be("c");
        }

        [Test]
        public async Task DataFrameLangTest() {
            var script = "df.lang <- data.frame(col1=c('a','中'),col2=c('國','d'),row.names = c('マイクロソフト','row2'));";
            var expectation = new VariableExpectation() { Name = "df.lang", Value = "2 obs. of  2 variables", TypeName = "list", Class = "data.frame", HasChildren = true, CanShowDetail = true };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var rowRange = new Range(0, 2);
            var columnRange = new Range(0, 2);
            var grid = await _hostScript.Session.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

            grid.ColumnHeader.Range.Should().Be(columnRange);
            grid.ColumnHeader[0].Should().Be("col1");
            grid.ColumnHeader[1].Should().Be("col2");

            grid.RowHeader.Range.Should().Be(rowRange);
            grid.RowHeader[0].Should().Be("マイクロソフト");
            grid.RowHeader[1].Should().Be("row2");

            grid.Grid.Range.Should().Be(new GridRange(rowRange, columnRange));
            grid.Grid[0, 0].Should().Be("a");
            grid.Grid[0, 1].Should().Be("國");
            grid.Grid[1, 0].Should().Be("中");
            grid.Grid[1, 1].Should().Be("d");
        }

        [Test]
        public async Task DataFrameManyColumnTest() {
            var script = "df.manycolumn<-data.frame(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30);";
            var expectation = new VariableExpectation() { Name = "df.manycolumn", Value = "1 obs. of  30 variables", TypeName = "list", Class = "data.frame", HasChildren = true, CanShowDetail = true };
            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper);

            var children = await evaluation.GetChildrenAsync();
            children.Count.Should().BeGreaterOrEqualTo(30);
        }

        [Test]
        public async Task PromiseTest() {
            var script = "e <- (function(x, y, z) base::environment())(1,,3)";
            var expectation = new VariableExpectation() { Name = "e", Value = "<environment:", TypeName = "environment", Class = "environment", HasChildren = true, CanShowDetail = false };

            var x_expectation = new VariableExpectation() { Name = "x", Value = "1", TypeName = "<promise>", Class = "<promise>", HasChildren = false, CanShowDetail = false };
            var y_expectation = new VariableExpectation() { Name = "z", Value = "3", TypeName = "<promise>", Class = "<promise>", HasChildren = false, CanShowDetail = false };

            var evaluation = (VariableViewModel)await _hostScript.EvaluateAndAssert(
                script,
                expectation,
                VariableRHostScript.AssertEvaluationWrapper_ValueStartWith);

            var children = await evaluation.GetChildrenAsync();

            children.Count.Should().Be(2);
            VariableRHostScript.AssertEvaluationWrapper(children[0], x_expectation);
            VariableRHostScript.AssertEvaluationWrapper(children[1], y_expectation);
        }

        [Test]
        public async Task DoesNotExist() {
            // This is the equivalent of what we get when we fetch a variable
            // for a data grid after that variable is no longer available (rm or reset).
            var script = "idonotexist";
            var evaluationResult = await _hostScript.EvaluateAsync(script);
            evaluationResult.Name.Should().BeNull();
            evaluationResult.Expression.Should().Be("idonotexist");

            var model = new VariableViewModel(evaluationResult, Substitute.For<IServiceContainer>());
            model.TypeName.Should().BeNull();
            model.Value.Should().BeNull();
        }

        object[,] arrayTestData = new object[,] {
            { "array.empty <- array();", new VariableExpectation() { Name = "array.empty", Value = "NA", TypeName = "logical", Class = "array", HasChildren = false, CanShowDetail = false } },
            { "array.10 <- array(1:10);", new VariableExpectation() { Name = "array.10", Value = "int [1:10(1d)] 1 2 3 4 5 6 7 8 9 10", TypeName = "integer", Class = "array", HasChildren = true, CanShowDetail = true } },
            { "array.2x2 <- array(c('z', 'y', 'x', 'w'), dim = c(2, 2));", new VariableExpectation() { Name = "array.2x2", Value = "chr [1:2, 1:2] \"z\" \"y\" \"x\" \"w\"", TypeName = "character", Class = "matrix", HasChildren = true, CanShowDetail = true } },
            { "array.2x3x4 <- array(as.double(101:124), dim=c(2,3,4));", new VariableExpectation() { Name = "array.2x3x4", Value = "num [1:2, 1:3, 1:4] 101 102 103 104 105 106 107 108 109 110 ...", TypeName = "double", Class = "array", HasChildren = true, CanShowDetail = false } },
        };

        [Test]
        public Task ArrayTest() {
            return RunTest(arrayTestData);
        }

        object[,] functionTestData = new object[,] {
            {   "x <- lm;",
                new VariableExpectation() {
                Name = "x",
                Value = "function (formula, data, subset, weights, na.action, method = \"qr\", model = TRUE,",
                TypeName = "closure",
                Class = "function",
                HasChildren = false,
                CanShowDetail = true
                }
            },
        };

        [Test]
        public Task FunctionTest() {
            return RunTest(functionTestData);
        }

        private async Task RunTest(object[,] testData) {
            var testCount = testData.GetLength(0);

            for (var i = 0; i < testCount; i++) {
                await _hostScript.EvaluateAndAssert(
                    (string)testData[i, 0],
                    (VariableExpectation)testData[i, 1],
                    VariableRHostScript.AssertEvaluationWrapper);
            }
        }
    }
}
