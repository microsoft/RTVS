// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tree {
    [ExcludeFromCodeCoverage]
    public class HtmlTreeVisitorTest {
        private HtmlTree ParseHtml(string html) {
            HtmlParser parser = new HtmlParser();
            HtmlTree tree = new HtmlTree(new TextStream(html));
            tree.Build();
            return tree;
        }

        [Test]
        [Category.Html]
        public void HtmlTree_VisitorTest() {
            string html = "<html><head>" +
                            "<script>alert(\"boo!\")</script>" +
                            "<style>.a { color: red; }</style>" +
                            "</head>" +
                            "<body style=\"text:green\" onload=\"some_script\">" +
                            "</body></html>";

            HtmlTree tree = ParseHtml(html);
            var visitor = new TestVisitor();
            tree.RootNode.Accept(visitor, parameter: null);

            Assert.Equal(6, visitor.Count); // 5 + root node
        }

        [ExcludeFromCodeCoverage]
        class TestVisitor : IHtmlTreeVisitor {
            public int Count = 0;

            #region IHtmlTreeVisitor Members

            public bool Visit(ElementNode node, object param) {
                Count++;
                return true;
            }

            #endregion
        }
    }
}
