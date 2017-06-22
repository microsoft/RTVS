// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.ExecutionTracing;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test;
using Microsoft.R.Host.Client.Test.Fixtures;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.StackTracing;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.DataInspection.Test {
    [ExcludeFromCodeCoverage]
    public class ValuesTest : IAsyncLifetime {
        private readonly IRemoteBroker _remoteBroker;
        private const REvaluationResultProperties AllFields = unchecked((REvaluationResultProperties)~0);
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public ValuesTest(IServiceContainer services, IRemoteBroker remoteBroker, TestMethodFixture testMethod) {
            _remoteBroker = remoteBroker;
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _remoteBroker.ConnectAsync(_sessionProvider);
            await _session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [Test]
        [Category.R.DataInspection]
        public async Task BacktickNames() {
            var tracer = await _session.TraceExecutionAsync();
            await _session.ExecuteAsync("`123` <- list(`name with spaces` = 42)");
            var stackFrames = (await _session.TracebackAsync()).ToArray();
            stackFrames.Should().NotBeEmpty();

            var children = await stackFrames.Last().DescribeChildrenAsync(ExpressionProperty | LengthProperty, RValueRepresentations.Deparse());
            var parent = children.Should().Contain(er => er.Name == "`123`")
                .Which.Should().BeAssignableTo<IRValueInfo>().Which;
            parent.Expression.Should().Be("`123`");

            children = await parent.DescribeChildrenAsync(ExpressionProperty, RValueRepresentations.Deparse());
            children.Should().Contain(er => er.Name == "$`name with spaces`")
                .Which.Should().BeAssignableTo<IRValueInfo>()
                .Which.Expression.Should().Be("`123`$`name with spaces`");
        }

        [Test]
        [Category.R.DataInspection]
        public async Task Promise() {
            const string code =
@"f <- function(p) {
    browser()
    force(p)
    browser()
  }
  f(1 + 2)";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await _session.TracebackAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();
                var frame = stackFrames.Last();

                var children = await frame.DescribeChildrenAsync(REvaluationResultProperties.None, RValueRepresentations.Deparse());
                children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeAssignableTo<IRPromiseInfo>()
                    .Which.Code.Should().Be("1 + 2");

                await tracer.ContinueAsync();
                await _session.NextPromptShouldBeBrowseAsync();

                children = await frame.DescribeChildrenAsync(REvaluationResultProperties.None, RValueRepresentations.Deparse());
                children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeAssignableTo<IRValueInfo>()
                    .Which.Representation.Should().Be("3");
            }
        }


        [Test]
        [Category.R.DataInspection]
        public async Task ActiveBinding() {
            const string code =
@"makeActiveBinding('x', function() 42, environment());
  browser();
";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await _session.TracebackAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var children = await stackFrames.Last().DescribeChildrenAsync(REvaluationResultProperties.None, RValueRepresentations.Deparse());
                children.Should().ContainSingle(er => er.Name == "x")
                    .Which.Should().BeAssignableTo<IRActiveBindingInfo>()
                    .Which.ComputedValue.Should().BeNull();
            }
        }

        [Test]
        [Category.R.DataInspection]
        public async Task ActiveBindingEvaluate() {
            const string code =
@"makeActiveBinding('x', function() 42, environment())
  browser();
";
            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await _session.TracebackAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var children = await stackFrames.Last().DescribeChildrenAsync(ComputedValueProperty, RValueRepresentations.Deparse());
                children.Should().ContainSingle(er => er.Name == "x")
                    .Which.Should().BeAssignableTo<IRActiveBindingInfo>()
                    .Which.ComputedValue.Representation.Should().Be("42");
            }
        }

        [Test]
        [Category.R.DataInspection]
        public async Task DimWithNames() {
            const string code =
@"x <- c(100, 200)
  dim(x) <- list(a = 1, b = 2)
";
            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await sf.Source(_session);

                var stackFrames = (await _session.TracebackAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var children = await stackFrames.Last().DescribeChildrenAsync(DimProperty, null);
                children.Should().ContainSingle(er => er.Name == "x")
                    .Which.Should().BeAssignableTo<IRValueInfo>()
                    .Which.Dim.Should().BeEquivalentTo(1L, 2L);
            }
        }

        [Test]
        [Category.R.DataInspection]
        public async Task MultilinePromise() {
            const string code =
@"f <- function(p, d) {
    force(d)
    browser()
  }
  x <- quote({{{}}})
  eval(substitute(f(P, x), list(P = x)))";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await _session.TracebackAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var children = (await stackFrames.Last().DescribeChildrenAsync(REvaluationResultProperties.None, RValueRepresentations.Deparse()));
                var d = children.Should().ContainSingle(er => er.Name == "d")
                    .Which.Should().BeAssignableTo<IRValueInfo>()
                    .Which;

                children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeAssignableTo<IRPromiseInfo>()
                    .Which.Code.Should().Be(d.Representation);
            }
        }

        [Test]
        [Category.R.DataInspection]
        public async Task EnvironmentIndependentResult() {
            const string code =
@"(function(p) {
    v <- 42
    makeActiveBinding('a', function() 42, environment())
    browser()
  })(42)";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await _session.TracebackAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var env = stackFrames.Last();
                var children = (await env.DescribeChildrenAsync(ExpressionProperty | ComputedValueProperty, RValueRepresentations.Deparse()));

                var v = children.Should().ContainSingle(er => er.Name == "v")
                    .Which.Should().BeAssignableTo<IRValueInfo>()
                    .Which;

                var p = children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeAssignableTo<IRPromiseInfo>()
                    .Which;

                var a = children.Should().ContainSingle(er => er.Name == "a")
                    .Which.Should().BeAssignableTo<IRActiveBindingInfo>()
                    .Which;

                var e = (await env.TryEvaluateAndDescribeAsync("non_existing_variable", None, null))
                    .Should().BeAssignableTo<IRErrorInfo>()
                    .Which;

                var iv = v.ToEnvironmentIndependentResult().Should().BeAssignableTo<IRValueInfo>().Which;
                (await _session.EvaluateAndDescribeAsync(iv.Expression, ExpressionProperty, RValueRepresentations.Deparse()))
                    .Should().BeAssignableTo<IRValueInfo>().Which.Representation.Should().Be(v.Representation);

                // When a promise expression is evaluated directly, rather than via children, the promise is forced
                // and becomes a value. To have something to compare it against, evaluate the original promise in
                // its original environment as well.
                var pv = await v.GetValueAsync(ExpressionProperty, RValueRepresentations.Deparse());
                var ipv = p.ToEnvironmentIndependentResult().Should().BeAssignableTo<IRPromiseInfo>().Which;
                (await _session.EvaluateAndDescribeAsync(ipv.Expression, ExpressionProperty, RValueRepresentations.Deparse()))
                    .Should().BeAssignableTo<IRValueInfo>()
                    .Which.Representation.Should().Be(pv.Representation);

                // When an active binding expression is evaluated directly, rather than via children, its active
                // binding nature is not discoverable, and it produces a value result.
                var iav = a.ToEnvironmentIndependentResult().Should().BeAssignableTo<IRActiveBindingInfo>().Which;
                (await _session.EvaluateAndDescribeAsync(iav.Expression, ExpressionProperty | ComputedValueProperty, RValueRepresentations.Deparse()))
                    .Should().BeAssignableTo<IRValueInfo>().Which.Representation.Should().Be(a.ComputedValue.Representation);

                var ie = e.ToEnvironmentIndependentResult().Should().BeAssignableTo<IRErrorInfo>().Which;
                (await _session.TryEvaluateAndDescribeAsync(ie.Expression, ExpressionProperty, RValueRepresentations.Deparse()))
                    .Should().BeAssignableTo<IRErrorInfo>();
            }
        }

        [CompositeTest]
        [Category.R.DataInspection]
        [InlineData("NULL", "NULL", " NULL", "")]
        [InlineData("TRUE", "TRUE", "TRUE", "TRUE")]
        [InlineData("FALSE", "FALSE", "FALSE", "FALSE")]
        [InlineData("NA", "NA", "NA", "NA")]
        [InlineData("NA_integer_", "NA_integer_", "NA", "NA")]
        [InlineData("NA_real_", "NA_real_", "NA", "NA")]
        [InlineData("NA_complex_", "NA_complex_", "NA", "NA")]
        [InlineData("NA_character_", "NA_character_", "NA", "NA")]
        [InlineData("+Inf", "Inf", "Inf", "Inf")]
        [InlineData("-Inf", "-Inf", "-Inf", "-Inf")]
        [InlineData("NaN", "NaN", "NaN", "NaN")]
        [InlineData("-NaN", "NaN", "NaN", "NaN")]
        [InlineData("+0", "0", "0", "0")]
        [InlineData("-0", "0", "0", "0")]
        [InlineData("0L", "0L", "0", "0")]
        [InlineData("42", "42", "42", "42")]
        [InlineData("42L", "42L", "42", "42")]
        [InlineData("-42", "-42", "-42", "-42")]
        [InlineData("-42L", "-42L", "-42", "-42")]
        [InlineData("42.24", "42.24", "42.2", "42.24")]
        [InlineData("-0.42", "-0.42", "-0.42", "-0.42")]
        [InlineData("4.2e10", "4.2e+10", "4.2e+10", "4.2e+10")]
        [InlineData("-4.2e10", "-4.2e+10", "-4.2e+10", "-4.2e+10")]
        [InlineData("4.2e-10", "4.2e-10", "4.2e-10", "4.2e-10")]
        [InlineData("-4.2e-10", "-4.2e-10", "-4.2e-10", "-4.2e-10")]
        [InlineData("0i", "0+0i", "0+0i", "0+0i")]
        [InlineData("4+2i", "4+2i", "4+2i", "4+2i")]
        [InlineData("4-2.4i", "4-2.4i", "4-2.4i", "4-2.4i")]
        [InlineData("-4.2+4i", "-4.2+4i", "-4.2+4i", "-4.2+4i")]
        [InlineData("-4.2-2.4i", "-4.2-2.4i", "-4.2-2.4i", "-4.2-2.4i")]
        //[InlineData("-4.2e+10+2.4e-10i", "-4.2e+10+2.4e-10i", "-4.2e+10+2.4e-10i", "-4.2e+10+2.4e-10i", Skip = "https://bugs.r-project.org/bugzilla/show_bug.cgi?id=16752")]
        [InlineData("''", "\"\"", "\"\"", "")]
        [InlineData(@"'abc'", @"""abc""", @"""abc""", "abc")]
        [InlineData(@"'\'\""\n\r\t\b\a\f\v\\\001'", @"""'\""\n\r\t\b\a\f\v\\\001""", @"""'\""\n\r\t\b\a\f\v\\\001""", "'\"\n\r\t\b\a\f\v\\\x01")]
        [InlineData(@"quote(sym)", @"sym", @"sym", "sym")]
        public async Task Representation(string expr, string deparse, string str, string toString) {
            string actualDeparse = (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.Deparse())).Representation;
            string actualStr = (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.Str())).Representation;
            string actualToString = (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.ToString)).Representation;

            actualDeparse.Should().Be(deparse);
            actualStr.Should().Be(str);
            actualToString.Should().Be(toString);
        }

        [CompositeTest]
        [Category.R.DataInspection]
        [InlineData(@"sQuote(dQuote('x'))", @"""‘“x”’""", @"""‘“x”’""", "‘“x”’", 1252)]
        [InlineData(@"'\u2260'", @"""<U+2260>""", @"""<U+2260>""""| __truncated__", "≠", 1252)]
        [InlineData("'Ûñïçôdè' ", @"""Ûñïçôdè""", @"""Ûñïçôdè""", @"Ûñïçôdè", 1252)]
        [InlineData("'Ûñïçôdè' ", @"""Unicode""", @"""Unicode""", @"Unicode", 1251)]
        public async Task RepresentationWithEncoding(string expr, string deparse, string str, string toString, int codepage) {
            await _session.SetCodePageAsync(codepage);
            await Representation(expr, deparse, str, toString);
        }

        [Test]
        [Category.R.DataInspection]
        public async Task DeparseLimit() {
            string expr = "as.double(1:100)";
            string fullRepr = (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.Deparse())).Representation;
            string expectedRepr = fullRepr.Substring(0, 53); 
            (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.Deparse(50)))
                .Representation.Should().Be(expectedRepr);
        }

        [Test]
        [Category.R.DataInspection]
        public async Task StrLimit() {
            string expr = "as.double(1:100)";
            string fullRepr = (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.Str())).Representation;
            string expectedRepr = fullRepr.Substring(0, 7) + "!!!";
            (await _session.EvaluateAndDescribeAsync(expr, None, RValueRepresentations.Str(10, null, "!!!")))
                .Representation.Should().Be(expectedRepr);
        }

        private const string Postfix = "<POSTFIX>";

        public class ChildrenDataRow : IEnumerable, IXunitSerializable {
            public string Expression { get; private set; }
            public int Length { get; private set; }
            public int NameCount { get; private set; }
            public int AttrCount { get; private set; }
            public int SlotCount { get; private set; }
            public bool Sorted { get; private set; }
            public object[][] Children { get; private set; } = new object[][] { };

            public ChildrenDataRow() {
            }

            public ChildrenDataRow(string expression, int length, int nameCount = 0, int attrCount = 0, int slotCount = 0, bool sorted = false) {
                Expression = expression;
                Length = length;
                NameCount = nameCount;
                AttrCount = attrCount;
                SlotCount = slotCount;
                Sorted = sorted;
            }

            public void Add(string name, string expression, string deparse) {
                Children = Children.Concat(new[] { new object[] { name, expression, deparse } }).ToArray();
            }

            public void Add(string name, string deparse) => Add(name, "PARENT" + name, deparse);

            public static implicit operator object[] (ChildrenDataRow row) =>
                new object[] { row };

            // Class must implement IEnumerable in order to support collection initializers. However, for those classes
            // that do that, when producing the signature of the test, xUnit will ignore ToString and just format them
            // as a list, element by element. Since we want our ToString to show up, instead of the children, implement
            // IEnumerable by returning our own ToString. 
            IEnumerator IEnumerable.GetEnumerator() =>
                new[] { ToString() }.GetEnumerator();

            public override string ToString() => Expression;

            void IXunitSerializable.Serialize(IXunitSerializationInfo info) {
                info.AddValue(nameof(Expression), Expression);
                info.AddValue(nameof(Length), Length);
                info.AddValue(nameof(NameCount), NameCount);
                info.AddValue(nameof(AttrCount), AttrCount);
                info.AddValue(nameof(SlotCount), SlotCount);
                info.AddValue(nameof(Sorted), Sorted);
                info.AddValue(nameof(Children), Children);
            }

            void IXunitSerializable.Deserialize(IXunitSerializationInfo info) {
                Expression = info.GetValue<string>(nameof(Expression));
                Length = info.GetValue<int>(nameof(Length));
                NameCount = info.GetValue<int>(nameof(NameCount));
                AttrCount = info.GetValue<int>(nameof(AttrCount));
                SlotCount = info.GetValue<int>(nameof(SlotCount));
                Sorted = info.GetValue<bool>(nameof(Sorted));
                Children = info.GetValue<object[][]>(nameof(Children));
            }
        }

        // Parent expression is assigned to variable "PARENT" after being evaluated, and children are retrieved
        // from that variable, so the corresponding expressions should use "PARENT" for the parent expression.
        // For example, if the parent expression is "c(1, 2)", and we're matching the first child, the expression
        // for that child should be written as "PARENT[[1]]".
        //
        // Furthermore, if child expression is not specified at all, then it is assumed to be "PARENT" followed 
        // by child name - e.g. if the name of the first child is "[[1]]", then, instead of using "PARENT[[1]]",
        // for the child expression, one can just omit it for the same effect.
        //
        // Due to a bug in the test discoverer, rows containing single quotes are not handled right, so
        // only double quotes should be used for string literals in expressions below.
        public static readonly object[][] ChildrenData = {
            new ChildrenDataRow("NULL", 0),
            new ChildrenDataRow("NA", 1),
            new ChildrenDataRow("NA_integer_", 1),
            new ChildrenDataRow("NA_real_", 1),
            new ChildrenDataRow("NA_complex_", 1),
            new ChildrenDataRow("NA_character_", 1),
            new ChildrenDataRow("+Inf", 1),
            new ChildrenDataRow("-Inf", 1),
            new ChildrenDataRow("NaN", 1),
            new ChildrenDataRow("TRUE", 1),
            new ChildrenDataRow("42L", 1),
            new ChildrenDataRow("42", 1),
            new ChildrenDataRow("42i", 1),
            new ChildrenDataRow(@"""str""", 1),
            new ChildrenDataRow("quote(sym)", 1),
            new ChildrenDataRow("TRUE[FALSE]", 0),
            new ChildrenDataRow("42[FALSE]", 0),
            new ChildrenDataRow("42L[FALSE]", 0),
            new ChildrenDataRow("42i[FALSE]", 0),
            new ChildrenDataRow(@"""str""[FALSE]", 0),
            new ChildrenDataRow("list()", 0),
            new ChildrenDataRow("pairlist()", 0),
            new ChildrenDataRow("as.environment(list())", 0),
            new ChildrenDataRow("c(TRUE, FALSE, NA)", 3) {
                { "[[1]]", "TRUE" },
                { "[[2]]", "FALSE" },
                { "[[3]]", "NA" },
            },
            new ChildrenDataRow("c(1L, 2L, NA_integer_)", 3) {
                { "[[1]]", "1L" },
                { "[[2]]", "2L" },
                { "[[3]]", "NA_integer_" },
            },
            new ChildrenDataRow("c(1, 2, NA)", 3) {
                { "[[1]]", "1" },
                { "[[2]]", "2" },
                { "[[3]]", "NA_real_" },
            },
            new ChildrenDataRow("c(1i, 2i, NA_complex_)", 3) {
                { "[[1]]", "0+1i" },
                { "[[2]]", "0+2i" },
                { "[[3]]", "NA_complex_" },
            },
            new ChildrenDataRow(@"c(""1"", ""2"", NA_character_)", 3) {
                { "[[1]]", @"""1""" },
                { "[[2]]", @"""2""" },
                { "[[3]]", "NA_character_" },
            },
            new ChildrenDataRow("c(1, x = 2, 3, `y \n z` = 4, x = 5)", 5, nameCount: 5, attrCount: 1) {
                { "[[1]]", "1" },
                { "[[\"x\"]]", "2" },
                { "[[3]]", "3" },
                { "[[\"y \\n z\"]]", "4" },
                { "[[5]]", "5" },
            },
            new ChildrenDataRow("c(x = 1)", 1, nameCount: 1, attrCount: 1) {
                { "[[\"x\"]]", "1" },
            },
            new ChildrenDataRow("list(1, TRUE, NA, NULL, list())", 5) {
                { "[[1]]", "1" },
                { "[[2]]", "TRUE" },
                { "[[3]]", "NA" },
                { "[[4]]", "NULL" },
                { "[[5]]", "list()" },
            },
            new ChildrenDataRow("list(1, x = 2, 3, `y \n z` = 4, x = 5)", 5, nameCount: 5, attrCount: 1) {
                { "[[1]]", "1" },
                { "$x", "2" },
                { "[[3]]", "3" },
                { "$`y \\n z`", "4" },
                { "[[5]]", "5" },
            },
            new ChildrenDataRow("pairlist(1, TRUE, NA, NULL, list())", 5) {
                { "[[1]]", "1" },
                { "[[2]]", "TRUE" },
                { "[[3]]", "NA" },
                { "[[4]]", "NULL" },
                { "[[5]]", "list()" },
            },
            new ChildrenDataRow("pairlist(1, x = 2, 3, `y \n z` = 4, x = 5)", 5, nameCount: 5, attrCount: 1) {
                { "[[1]]", "1" },
                { "$x", "2" },
                { "[[3]]", "3" },
                { "$`y \\n z`", "4" },
                { "[[5]]", "5" },
            },
            new ChildrenDataRow("as.environment(list(x = 1, `y \n z` = NA, n = NULL, e = .GlobalEnv))", 4, nameCount: 4, sorted: true) {
                { "`y \\n z`", "PARENT$`y \\n z`", "NA" },
                { "e", "PARENT$e", "<environment>" },
                { "n", "PARENT$n", "NULL" },
                { "x", "PARENT$x", "1" },
            },
            new ChildrenDataRow("setClass(\"C\", slots = list(x = \"numeric\", `y \n z` = \"logical\"))(x = 1, `y \n z` = NA)", 1, slotCount: 2, attrCount: 3) {
                { "@x", "1" },
                { "@`y \\n z`", "NA" },
            },
            new ChildrenDataRow("setClass(\"C\", contains = \"list\", slots = list(y = \"numeric\"))(list(x = 1), y = 2)", 1, nameCount: 1, slotCount: 2, attrCount: 3) {
                { "@.Data", "list(1)" },
                { "@y", "2" },
                { "$x", "1" },
            },
        };

        [CompositeTest]
        [Category.R.DataInspection]
        [MemberData(nameof(ChildrenData))]
        public async Task Children(ChildrenDataRow row) {
            var children = row.Children.Select(childRow => {
                var child = Substitute.For<IRValueInfo>();
                child.Name.Returns((string)childRow[0]);
                child.Expression.Returns((string)childRow[1]);
                child.Representation.Returns((string)childRow[2]);
                return child;
            }).ToArray();

            var frame = (await _session.TracebackAsync()).Single();

            await _session.ExecuteAsync("PARENT <- {" + row.Expression + "}");

            var res = (await frame.TryEvaluateAndDescribeAsync("PARENT", AllFields, null))
                .Should().BeAssignableTo<IRValueInfo>().Which;
            res.Length.Should().Be(row.Length);
            res.NameCount.Should().Be(row.NameCount);
            res.AttributeCount.Should().Be(row.AttrCount);
            res.SlotCount.Should().Be(row.SlotCount);

            var actualChildren = (await res.DescribeChildrenAsync(AllFields, RValueRepresentations.Deparse()))
                .Cast<IRValueInfo>()
                .ToArray();
            res.HasChildren.Should().Be(children.Any());

            if (row.Sorted) {
                actualChildren = actualChildren.OrderBy(er => er.Name).ToArray();
            }

            actualChildren.ShouldAllBeEquivalentTo(children, options => options
                .Including(child => child.Name)
                .Including(child => child.Expression)
                .Including(child => child.Representation)
                .WithStrictOrdering());
        }
    }
}
