// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatScopeTest {
        [Test]
        public void Formatter_EmptyFileTest() {
            RFormatter f = new RFormatter();
            string s = f.Format(string.Empty);
            s.Should().BeEmpty();
        }

        [Test]
        public void Formatter_FormatRandom01() {
            RFormatter f = new RFormatter();
            string original = "a   b 1.  2 Inf\tNULL";

            string actual = f.Format(original);

            actual.Should().Be(@"a b 1. 2 Inf NULL");
        }

        [Test]
        public void Formatter_StatementTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x<-2");
            string expected = "x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        public void Formatter_FormatSimpleScopesTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("{\n{}}");
            string expected =
"{\n" +
"  { }\n" +
"}";
            actual.Should().Be(expected);
        }

        [Test]
        public void Formatter_FormatSimpleScopesTest02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("{\n{\n}}");
            string expected =
    "{\n" +
    "  { }\n" +
    "}";
            actual.Should().Be(expected);
        }

        [Test]
        public void Formatter_FormatSimpleScopesTest03() {
            RFormatter f = new RFormatter();
            string actual = f.Format("{{}}");
            string expected =
    "{\n" +
    "  { }\n" +
    "}";
            actual.Should().Be(expected);
        }
    }
}
