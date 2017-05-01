// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Tree.Builder;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tree {
    [ExcludeFromCodeCoverage]
    public class DefaultHtmlInfoProviderTest {
        [Test]
        [Category.Html]
        public void IsImplicitlyClosedTest() {
            string[] implicitlyClosingElements = new string[]{
               "frame",
                "dd",
                "dt",
                "li",
                "Option",
                "p",
                "tbOdy",
                "tfoot",
                "TD",
                "th",
                "thead",
                "tr",
            };

            string[] notImplicitlyClosingElements = new string[]{
               "html",
                "123",
                " ",
                "!",
                "абвгд",
             };


            var target = new DefaultHtmlClosureProvider();
            bool actual;

            foreach (string s in implicitlyClosingElements) {
                string[] containerNames;

                actual = target.IsImplicitlyClosed(new TextStream(s), TextRange.FromBounds(0, s.Length), out containerNames);
                Assert.True(actual);
            }

            foreach (string s in notImplicitlyClosingElements) {
                string[] containerNames;

                actual = target.IsImplicitlyClosed(new TextStream(s), TextRange.FromBounds(0, s.Length), out containerNames);
                Assert.False(actual);
            }
        }

        [Test]
        [Category.Html]
        public void IsSelfClosingTest() {
            string[] selfClosingElements = new string[]{
                "area",
                "base",
                "basefont",
                "br",
                "col",
                "command",
                "embed",
                "hr",
                "img",
                "input",
                "isindex",
                "link",
                "meta",
                "param",
                "source",
                "track",
                "wbr",
            };

            string[] notSelfClosingElements = new string[]{
               "html",
                "123",
                " ",
                "!",
                "абвгд",
             };


            var target = new DefaultHtmlClosureProvider();
            bool actual;

            foreach (string s in selfClosingElements) {
                actual = target.IsSelfClosing(new TextStream(s), TextRange.FromBounds(0, s.Length));
                Assert.True(actual);
            }

            foreach (string s in notSelfClosingElements) {
                actual = target.IsSelfClosing(new TextStream(s), TextRange.FromBounds(0, s.Length));
                Assert.False(actual);
            }
        }
    }
}
