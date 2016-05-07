// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser.States {
    [ExcludeFromCodeCoverage]
    public class EntityParseTest {
        [Test]
        [Category.Html]
        public void OnEntity_WellFormedTest1() {
            HtmlParser parser = new HtmlParser();

            parser.EntityFound += delegate (object sender, HtmlParserRangeEventArgs args) {
                Assert.Equal(3, args.Range.Start);
                Assert.Equal(9, args.Range.End);
            };

            parser.Parse("foo&nbsp;");
        }

        [Test]
        [Category.Html]
        public void OnEntity_WellFormedTest2() {
            HtmlParser parser = new HtmlParser();
            int count = 0;

            parser.EntityFound += delegate (object sender, HtmlParserRangeEventArgs args) {
                count++;
            };

            parser.Parse("foo&nbsp;bar &lt;<html &lt; dir=\"&gt;\"> &gt; &quot");

            Assert.Equal(3, count);
        }

        [Test]
        [Category.Html]
        public void OnEntity_WellFormedTest3() {
            HtmlParser parser = new HtmlParser();
            int count = 0;

            parser.EntityFound += delegate (object sender, HtmlParserRangeEventArgs args) {
                count++;
            };

            parser.Parse(new TextStream("foo&#173;bar &#xabc;&abc;<html &lt; dir=\"&gt;"));

            Assert.Equal(2, count);
        }

        [Test]
        [Category.Html]
        public void OnEntity_BrokenTest() {
            HtmlParser parser = new HtmlParser();
            int count = 0;

            parser.EntityFound += delegate (object sender, HtmlParserRangeEventArgs args) {
                count++;
            };

            parser.Parse("&gt");

            Assert.Equal(0, count);
        }
    }
}
