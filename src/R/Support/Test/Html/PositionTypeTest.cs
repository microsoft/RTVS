// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    public class HtmlTree_PositionTypeTests {
        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests1() {
            string html = "<html dir=\"rtl\"> </html>";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.InStartTag,
                HtmlPositionType.InContent,
                HtmlPositionType.InContent,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InContent,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests2() {
            string html = "<html dir=rtl> </html>";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.InContent,
                HtmlPositionType.InContent,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InContent,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests3() {
            string html = "<a dir=\"> </a>";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.InContent,
                HtmlPositionType.InContent,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests4() {
            string html = "<a dir=> </a>";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.InContent,
                HtmlPositionType.InContent,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InEndTag,
                HtmlPositionType.InContent,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests5() {
            string html = "<a dir=";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests6() {
            string html = "<a dir=\"";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.AfterEqualsSign,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests7() {
            string html = "<a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests8() {
            string html = "<a ";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.InStartTag,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests9() {
            string html = "<a <a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.InStartTag,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests10() {
            string html = "<a dir=<a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests11() {
            string html = "<a dir=\"<a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests12() {
            string html = "<a dir=\"r<a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.AttributeName,
                HtmlPositionType.EqualsSign,
                HtmlPositionType.AfterEqualsSign,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.AttributeValue,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests13() {
            string html = "<a /<a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.InStartTag,
                HtmlPositionType.InStartTag,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        [Test]
        [Category.Html]
        public void HtmlTree_PositionTypeTests14() {
            string html = "<a <a";
            var positionTypes = new HtmlPositionType[]{
                HtmlPositionType.InContent,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
                HtmlPositionType.InStartTag,
                HtmlPositionType.ElementName,
                HtmlPositionType.ElementName,
            };

            VerifyPositions(html, positionTypes);
        }

        private void VerifyPositions(string html, HtmlPositionType[] positionTypes) {
            var tree = new HtmlTree(new TextStream(html), null, null, ParsingMode.Html);
            tree.Build();

            for (int i = 0; i < html.Length; i++) {
                ElementNode element;
                AttributeNode attribute;

                var pos = tree.RootNode.GetPositionElement(i, out element, out attribute);
                Assert.Equal(positionTypes[i], pos);
            }
        }
    }
}

