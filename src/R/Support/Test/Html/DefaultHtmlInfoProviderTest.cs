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
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

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
