// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.DataTips;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.DataTips {
    [ExcludeFromCodeCoverage]
    public class DataTipTest {
        [CompositeTest]
        [Category.R.DataInspection]
        [MemberData(nameof(Everywhere), "NULL")]
        [MemberData(nameof(Everywhere), "NA")]
        [MemberData(nameof(Everywhere), "42")]
        [MemberData(nameof(Everywhere), "42.24")]
        [MemberData(nameof(Everywhere), "'a'")]
        [MemberData(nameof(Everywhere), "\"a\"")]
        [MemberData(nameof(Everywhere), "`a`")]
        [MemberData(nameof(Everywhere), "a")]
        [MemberData(nameof(Everywhere), "a$b")]
        [MemberData(nameof(Everywhere), "a@b")]
        [MemberData(nameof(Everywhere), "a::b")]
        [MemberData(nameof(Everywhere), "a:::b")]
        [MemberData(nameof(EverywhereExcept), "a[b]", 2)]
        [MemberData(nameof(EverywhereExcept), "a[[b]]", 3)]
        [InlineData(" NULL ", 0, 0, null)]
        [InlineData(" NULL ", 0, 1, null)]
        [InlineData(" NULL ", 4, 2, null)]
        [InlineData(" NULL ", 5, 0, null)]
        [InlineData(" NULL ", 5, 1, null)]
        [InlineData("a::b$c@d[e$f]", 0, 0, "a::b$c@d[e$f]")]
        [InlineData("a::b$c@d[e$f]", 5, 0, "a::b$c@d[e$f]")]
        [InlineData("a::b$c@d[e$f]", 9, 0, "e$f")]
        [InlineData("a[[b$c + d:::e@f]]", 0, 0, "a[[b$c + d:::e@f]]")]
        [InlineData("a[[b$c + d:::e@f]]", 3, 0, "b$c")]
        [InlineData("a[[b$c + d:::e@f]]", 7, 0, null)]
        [InlineData("a[[b$c + d:::e@f]]", 9, 0, "d:::e@f")]
        public void DataTip(string code, int start, int length, string dataTip) {
            var ast = RParser.Parse(new TextStream(code));
            var node = RDataTip.GetDataTipExpression(ast, new TextRange(start, length));
            if (dataTip == null) {
                node.Should().BeNull();
            } else {
                node.Should().NotBeNull();
                var expr = code.Substring(node.Start, node.Length);
                expr.Should().Be(dataTip);
            }
        }

        public static IEnumerable<object[]> Everywhere(string code) {
            for (int i = 0; i < code.Length - 1; ++i) {
                for (int j = i; j < code.Length; ++j) {
                    yield return new object[] { code, i, j - i, code };
                }
            }
        }

        public static IEnumerable<object[]> EverywhereExcept(string code, int except) {
            return Everywhere(code).Where(args => (int)args[1] != except);
        }
    }
}