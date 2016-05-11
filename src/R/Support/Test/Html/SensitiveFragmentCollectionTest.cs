// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Tree;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tree {
    [ExcludeFromCodeCoverage]
    public class SentitiveFragmentCollectionTest {
        private HtmlTree ParseHtml(string html) {
            HtmlParser parser = new HtmlParser();
            HtmlTree tree = new HtmlTree(new TextStream(html));
            tree.Build();
            return tree;
        }

        [Test]
        [Category.Html]
        public void SentitiveFragmentCollection_IsDestructiveChange_Insert() {
            string html = "<!--   -->";
            bool result;

            HtmlTree tree = ParseHtml(html);

            var cc = tree.CommentCollection;
            result = cc.IsDestructiveChange(0, 0, 1, new TextStream(html), new TextStream("a<!--   -->"));
            Assert.False(result);

            result = cc.IsDestructiveChange(1, 0, 1, new TextStream(html), new TextStream("<a!--   -->"));
            Assert.True(result);

            result = cc.IsDestructiveChange(4, 0, 1, new TextStream(html), new TextStream("<!--a -->"));
            Assert.True(result);

            result = cc.IsDestructiveChange(5, 0, 1, new TextStream(html), new TextStream("<!-- a  -->"));
            Assert.False(result);

            result = cc.IsDestructiveChange(7, 0, 1, new TextStream(html), new TextStream("<!--   a-->"));
            Assert.False(result);

            result = cc.IsDestructiveChange(8, 0, 1, new TextStream(html), new TextStream("<!--   -a->"));
            Assert.True(result);

            result = cc.IsDestructiveChange(10, 0, 1, new TextStream(html), new TextStream("<!--   -->a"));
            Assert.False(result);
        }

        [Test]
        [Category.Html]
        public void SentitiveFragmentCollection_IsDestructiveChange_Delete() {
            string html = "<!--   --> ";
            bool result;

            HtmlTree tree = ParseHtml(html);

            var cc = tree.CommentCollection;
            result = cc.IsDestructiveChange(0, 1, 0, new TextStream(html), new TextStream("!--   --> "));
            Assert.True(result);

            result = cc.IsDestructiveChange(1, 1, 0, new TextStream(html), new TextStream("<--   --> "));
            Assert.True(result);

            result = cc.IsDestructiveChange(4, 1, 0, new TextStream(html), new TextStream("<!--  --> "));
            Assert.True(result);

            result = cc.IsDestructiveChange(5, 1, 0, new TextStream(html), new TextStream("<!--  --> "));
            Assert.False(result);

            result = cc.IsDestructiveChange(7, 1, 0, new TextStream(html), new TextStream("<!--   -> "));
            Assert.True(result);

            result = cc.IsDestructiveChange(10, 1, 0, new TextStream(html), new TextStream("<!--   -->"));
            Assert.False(result);
        }

        [Test]
        [Category.Html]
        public void SentitiveFragmentCollection_IsDestructiveChange_Replace() {
            string html = "<!--   -->abc";
            bool result;

            HtmlTree tree = ParseHtml(html);

            var cc = tree.CommentCollection;
            result = cc.IsDestructiveChange(0, 1, 2, new TextStream(html), new TextStream("ab!--   -->abc"));
            Assert.True(result);

            result = cc.IsDestructiveChange(0, 2, 1, new TextStream(html), new TextStream("a--   -->abc"));
            Assert.True(result);

            result = cc.IsDestructiveChange(3, 1, 2, new TextStream(html), new TextStream("<!-ab   -->abc"));
            Assert.True(result);

            result = cc.IsDestructiveChange(1, 3, 1, new TextStream(html), new TextStream("<a   -->abc"));
            Assert.True(result);

            result = cc.IsDestructiveChange(4, 1, 3, new TextStream(html), new TextStream("<!--abc  -->abc"));
            Assert.True(result);

            result = cc.IsDestructiveChange(5, 1, 3, new TextStream(html), new TextStream("<!-- abc -->abc"));
            Assert.False(result);

            result = cc.IsDestructiveChange(7, 2, 1, new TextStream(html), new TextStream("<!--   a>abc"));
            Assert.True(result);

            result = cc.IsDestructiveChange(10, 1, 2, new TextStream(html), new TextStream("<!--   -->aabc"));
            Assert.False(result);

            result = cc.IsDestructiveChange(11, 2, 4, new TextStream(html), new TextStream("<!--   -->abcde"));
            Assert.False(result);

            result = cc.IsDestructiveChange(9, 3, 3, new TextStream(html), new TextStream("<!--   --xabc"));
            Assert.True(result);
        }

        [Test]
        [Category.Html]
        public void SentitiveFragmentCollection_IsDestructiveChange_Unterminated() {
            string html = "<!-- --";
            bool result;

            HtmlTree tree = ParseHtml(html);

            var cc = tree.CommentCollection;
            result = cc.IsDestructiveChange(5, 0, 1, new TextStream(html), new TextStream("<!-- ---"));
            Assert.False(result);

            result = cc.IsDestructiveChange(7, 0, 1, new TextStream(html), new TextStream("<!-- ---"));
            Assert.False(result);

            result = cc.IsDestructiveChange(7, 0, 1, new TextStream(html), new TextStream("<!-- -->"));
            Assert.True(result);

            result = cc.IsDestructiveChange(5, 0, 3, new TextStream(html), new TextStream("<!-- -->--"));
            Assert.True(result);
        }

        [Test]
        [Category.Html]
        public void SentitiveFragmentCollection_IsDestructiveChange_Multiple() {
            string html = "<!-- --><!-- ";
            bool result;

            HtmlTree tree = ParseHtml(html);

            var cc = tree.CommentCollection;
            result = cc.IsDestructiveChange(5, 4, 1, new TextStream(html), new TextStream("<!-- a!-- "));
            Assert.True(result);

            result = cc.IsDestructiveChange(0, html.Length, 0, new TextStream(html), new TextStream(String.Empty));
            Assert.True(result);
        }
    }
}
