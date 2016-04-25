// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Match;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class ValuesTest : IAsyncLifetime {
        private const DebugEvaluationResultFields AllFields = unchecked((DebugEvaluationResultFields)~0);

        private readonly MethodInfo _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public ValuesTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _sessionProvider = new RSessionProvider();
            _session = _sessionProvider.GetOrCreate(Guid.NewGuid());
        }

        public async Task InitializeAsync() {
            await _session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = RUtilities.FindExistingRBasePath()
            }, new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [Test]
        [Category.R.Debugger]
        public async Task BacktickNames() {
            using (var debugSession = new DebugSession(_session)) {
                await _session.EvaluateAsync("`123` <- list(`name with spaces` = 42)", REvaluationKind.Mutating);
                var stackFrames = (await debugSession.GetStackFramesAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var frame = (await stackFrames.Last().GetEnvironmentAsync()).Should().BeOfType<DebugValueEvaluationResult>().Which;
                var children = await frame.GetChildrenAsync(DebugEvaluationResultFields.Expression | DebugEvaluationResultFields.ReprDeparse | DebugEvaluationResultFields.Length);
                var parent = children.Should().Contain(er => er.Name == "`123`")
                    .Which.Should().BeOfType<DebugValueEvaluationResult>().Which;
                parent.Expression.Should().Be("`123`");

                children = await parent.GetChildrenAsync(DebugEvaluationResultFields.Expression | DebugEvaluationResultFields.ReprDeparse);
                children.Should().Contain(er => er.Name == "$`name with spaces`")
                    .Which.Should().BeOfType<DebugValueEvaluationResult>()
                    .Which.Expression.Should().Be("`123`$`name with spaces`");
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task Promise() {
            const string code =
@"f <- function(p) {
    browser()
    force(p)
    browser()
  }
  f(1 + 2)";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await debugSession.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await debugSession.GetStackFramesAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var frame = (await stackFrames.Last().GetEnvironmentAsync()).Should().BeOfType<DebugValueEvaluationResult>().Which;
                var children = await frame.GetChildrenAsync(DebugEvaluationResultFields.ReprDeparse);
                children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeOfType<DebugPromiseEvaluationResult>()
                    .Which.Code.Should().Be("1 + 2");

                await debugSession.ContinueAsync();
                await debugSession.NextPromptShouldBeBrowseAsync();

                children = await frame.GetChildrenAsync(DebugEvaluationResultFields.ReprDeparse);
                children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeOfType<DebugValueEvaluationResult>()
                    .Which.GetRepresentation().Deparse.Should().Be("3");
            }
        }


        [Test]
        [Category.R.Debugger]
        public async Task ActiveBinding() {
            const string code =
@"makeActiveBinding('x', function() 42, environment());
  browser();
";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await debugSession.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await debugSession.GetStackFramesAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var frame = (await stackFrames.Last().GetEnvironmentAsync()).Should().BeOfType<DebugValueEvaluationResult>().Which;
                var children = await frame.GetChildrenAsync(DebugEvaluationResultFields.ReprDeparse);
                children.Should().ContainSingle(er => er.Name == "x")
                    .Which.Should().BeOfType<DebugActiveBindingEvaluationResult>();
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task MultilinePromise() {
            const string code =
@"f <- function(p, d) {
    force(d)
    browser()
  }
  x <- quote({{{}}})
  eval(substitute(f(P, x), list(P = x)))";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await debugSession.NextPromptShouldBeBrowseAsync();

                var stackFrames = (await debugSession.GetStackFramesAsync()).ToArray();
                stackFrames.Should().NotBeEmpty();

                var frame = (await stackFrames.Last().GetEnvironmentAsync()).Should().BeOfType<DebugValueEvaluationResult>().Which;
                var children = (await frame.GetChildrenAsync(DebugEvaluationResultFields.ReprDeparse));
                var d = children.Should().ContainSingle(er => er.Name == "d")
                    .Which.Should().BeOfType<DebugValueEvaluationResult>()
                    .Which;

                var p = children.Should().ContainSingle(er => er.Name == "p")
                    .Which.Should().BeOfType<DebugPromiseEvaluationResult>()
                    .Which.Code.Should().Be(d.GetRepresentation().Deparse);
            }
        }

        [CompositeTest]
        [Category.R.Debugger]
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
        //[InlineData(@"'\u2260'", @"""≠""", @"""≠""", "≠")]
        [InlineData(@"sQuote(dQuote('x'))", @"""‘“x”’""", @"""‘“x”’""", "‘“x”’")]
        [InlineData(@"quote(sym)", @"sym", @"sym", "sym")]
        public async Task Representation(string expr, string deparse, string str, string toString) {
            using (var debugSession = new DebugSession(_session)) {
                var res = (await debugSession.EvaluateAsync(expr, DebugEvaluationResultFields.ReprDeparse | DebugEvaluationResultFields.ReprStr | DebugEvaluationResultFields.ReprToString))
                    .Should().BeAssignableTo<DebugValueEvaluationResult>().Which;
                res.GetRepresentation().Should().Be(new MatchMembers<DebugValueRepresentation>()
                    .Matching(x => x.Deparse, deparse)
                    .Matching(x => x.Str, str)
                    .Matching(x => x.ToString, toString));
            }
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
                Children = Children.Append(new object[] { name, expression, deparse }).ToArray();
            }

            public void Add(string name, string deparse) {
                Add(name, "PARENT" + name, deparse);
            }

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
        [Category.R.Debugger]
        [MemberData(nameof(ChildrenData))]
        public async Task Children(ChildrenDataRow row) {
            var children = row.Children.Select(childRow =>
                MatchAny<DebugEvaluationResult>.ThatMatches(
                    new MatchMembers<DebugValueEvaluationResult>()
                        .Matching(er => er.Name, (string)childRow[0])
                        .Matching(er => er.Expression, (string)childRow[1])
                        .Matching(er => er.GetRepresentation().Deparse, (string)childRow[2])));

            using (var debugSession = new DebugSession(_session)) {
                var frame = (await debugSession.GetStackFramesAsync()).Single();

                (await frame.EvaluateAsync("PARENT <- {" + row.Expression + "}", DebugEvaluationResultFields.None))
                    .Should().BeOfType<DebugValueEvaluationResult>();

                var res = (await frame.EvaluateAsync("PARENT", AllFields))
                    .Should().BeOfType<DebugValueEvaluationResult>().Which;
                res.Length.Should().Be(row.Length);
                res.NameCount.Should().Be(row.NameCount);
                res.AttributeCount.Should().Be(row.AttrCount);
                res.SlotCount.Should().Be(row.SlotCount);

                var actualChildren = await res.GetChildrenAsync(AllFields);
                res.HasChildren.Should().Be(children.Any());

                if (row.Sorted) {
                    actualChildren = actualChildren.OrderBy(er => er.Name).ToArray();
                }
                actualChildren.Should().Equal(children);
            }
        }
    }
}
