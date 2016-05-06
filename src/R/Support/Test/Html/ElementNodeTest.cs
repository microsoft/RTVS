// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Html.Core.Tree.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tree {
    [ExcludeFromCodeCoverage]
    public class ElementNodeTest {
        private HtmlTree ParseHtml(string html) {
            HtmlParser parser = new HtmlParser();
            HtmlTree tree = new HtmlTree(new TextStream(html));
            tree.Build();
            return tree;
        }

        [Test]
        [Category.Html]
        public void ElementNode_GetPositionNodeTest() {
            string html = " <html dir=\"rtl\">foo</html>";

            HtmlTree tree = ParseHtml(html);

            ElementNode element;
            AttributeNode attribute;

            var position = tree.RootNode.GetPositionElement(0, out element, out attribute);
            Assert.NotNull(element);
            Assert.True(element is RootNode);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InContent, position);

            position = tree.RootNode.GetPositionElement(2, out element, out attribute);
            Assert.NotNull(element);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.ElementName, position);

            Assert.True(String.Compare(element.Name, "html") == 0);

            position = tree.RootNode.GetPositionElement(6, out element, out attribute);
            Assert.True(element is ElementNode);
            Assert.Equal(HtmlPositionType.ElementName, position);

            position = tree.RootNode.GetPositionElement(7, out element, out attribute);
            Assert.NotNull(element);
            Assert.NotNull(attribute);
            Assert.Equal(HtmlPositionType.InStartTag | HtmlPositionType.AttributeName, position);

            Assert.True(String.Compare(attribute.Name, "dir") == 0);

            position = tree.RootNode.GetPositionElement(11, out element, out attribute);
            Assert.NotNull(element);
            Assert.NotNull(attribute);
            Assert.Equal(HtmlPositionType.InStartTag | HtmlPositionType.AfterEqualsSign, position);

            Assert.True(String.Compare(attribute.Name, "dir") == 0);

            position = tree.RootNode.GetPositionElement(13, out element, out attribute);
            Assert.NotNull(element);
            Assert.NotNull(attribute);
            Assert.Equal(HtmlPositionType.InStartTag | HtmlPositionType.AttributeValue, position);

            position = tree.RootNode.GetPositionElement(19, out element, out attribute);
            Assert.NotNull(element);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InContent, position);

            position = tree.RootNode.GetPositionElement(24, out element, out attribute);
            Assert.NotNull(element);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InEndTag, position);
        }

        [Test]
        [Category.Html]
        public void ElementNode_GetPositionNodeTest_IncompleteEndTag() {
            ElementNode element;
            AttributeNode attribute;
            HtmlTree tree;
            HtmlPositionType position;
            string html;

            // Missing end tag at EOF
            html = "<html>";
            tree = ParseHtml(html);

            position = tree.RootNode.GetPositionElement(html.Length, out element, out attribute);
            Assert.NotNull(element);
            Assert.True(String.Compare(element.Name, "html") == 0);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InContent, position);

            // Partial end tag at EOF
            html = "<html></ht";
            tree = ParseHtml(html);

            position = tree.RootNode.GetPositionElement(html.Length, out element, out attribute);
            Assert.NotNull(element);
            Assert.True(String.Compare(element.Name, "html") == 0);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InContent, position);

            // shorthand
            html = "<html />";
            tree = ParseHtml(html);

            position = tree.RootNode.GetPositionElement(html.Length, out element, out attribute);
            Assert.NotNull(element);
            Assert.True(element is RootNode);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InContent, position);

            // self closing
            html = "<br>";
            tree = ParseHtml(html);

            position = tree.RootNode.GetPositionElement(html.Length, out element, out attribute);
            Assert.NotNull(element);
            Assert.True(element is RootNode);
            Assert.Null(attribute);
            Assert.Equal(HtmlPositionType.InContent, position);
        }

        [Test]
        [Category.Html]
        public void ElementNode_GetPositionNode_StyleScriptTest() {
            string html = "<html><head>" +
                            "<script>alert(\"boo!\")</script>" +
                            "<style>.a { color: red; }</style>" +
                            "</head>" +
                            "<body style=\"text:green\" onload=\"some_script\">" +
                            "</body></html>";

            HtmlTree tree = ParseHtml(html);

            ElementNode element;
            AttributeNode attribute;

            var position = tree.RootNode.GetPositionElement(25, out element, out attribute);
            Assert.NotNull(element);
            Assert.Null(attribute);
            Assert.True(element.IsScriptBlock());
            Assert.Equal(HtmlPositionType.InScriptBlock, position);

            position = tree.RootNode.GetPositionElement(50, out element, out attribute);
            Assert.NotNull(element);
            Assert.Null(attribute);
            Assert.True(element.IsStyleBlock());
            Assert.Equal(HtmlPositionType.InStyleBlock, position);

            position = tree.RootNode.GetPositionElement(105, out element, out attribute);
            Assert.NotNull(element);
            Assert.NotNull(attribute);
            Assert.True(attribute.IsStyleAttribute());
            Assert.Equal(HtmlPositionType.InInlineStyle, position);

            position = tree.RootNode.GetPositionElement(125, out element, out attribute);
            Assert.NotNull(element);
            Assert.NotNull(attribute);
            Assert.True(attribute.IsScriptAttribute());
            Assert.Equal(HtmlPositionType.InInlineScript, position);
        }

        [Test]
        [Category.Html]
        public void ElementNode_GetCommonAncestorTest() {
            string html = "<head><style></style></head><body><table><tr><td><ul><li><td><a></a></td></tr><tr></tr></table></body>";

            HtmlTree tree = ParseHtml(html);

            ElementNode head = tree.RootNode.Children[0];
            ElementNode body = tree.RootNode.Children[1];

            ElementNode style = head.Children[0];

            ElementNode table = body.Children[0];
            ElementNode tr1 = table.Children[0];
            ElementNode tr2 = table.Children[1];

            ElementNode td11 = tr1.Children[0];
            ElementNode td12 = tr1.Children[1];

            ElementNode ul = td11.Children[0];
            ElementNode li = ul.Children[0];

            ElementNode a = td12.Children[0];

            var node = tree.RootNode.GetCommonAncestor(head, body);
            Assert.True(node is RootNode);

            node = tree.RootNode.GetCommonAncestor(td11, td12);
            Assert.Equal("tr", node.Name);

            node = tree.RootNode.GetCommonAncestor(tr1, tr2);
            Assert.Equal("table", node.Name);

            node = tree.RootNode.GetCommonAncestor(tr1, a);
            Assert.Equal("tr", node.Name);

            node = tree.RootNode.GetCommonAncestor(table, a);
            Assert.Equal("table", node.Name);

            node = tree.RootNode.GetCommonAncestor(ul, a);
            Assert.Equal("tr", node.Name);

            node = tree.RootNode.GetCommonAncestor(head, a);
            Assert.True(node is RootNode);

            node = tree.RootNode.GetCommonAncestor(ul, body);
            Assert.Equal("body", node.Name);

            node = tree.RootNode.GetCommonAncestor(li, style);
            Assert.True(node is RootNode);

            node = tree.RootNode.GetCommonAncestor(ul, li);
            Assert.Equal("ul", node.Name);
        }
    }
}
