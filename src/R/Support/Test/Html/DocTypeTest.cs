// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test {
    [ExcludeFromCodeCoverage]
    public class HtmlParser_DocType {
        [Test]
        [Category.Html]
        public void HtmlParser_KnownDocTypes() {
            var parser = new HtmlParser();

            parser.Parse(DocTypeSignatures.Html32 + "<html></html>");
            Assert.Equal(DocType.Html32, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Html401Frameset + "<html></html>");
            Assert.Equal(DocType.Html401Frameset, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Html401Strict + "<html></html>");
            Assert.Equal(DocType.Html401Strict, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Html401Transitional + "<html></html>");
            Assert.Equal(DocType.Html401Transitional, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Html5 + "<html></html>");
            Assert.Equal(DocType.Html5, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Xhtml10Frameset + "<html></html>");
            Assert.Equal(DocType.Xhtml10Frameset, parser.DocType);
            Assert.Equal(ParsingMode.Xhtml, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Xhtml10Strict + "<html></html>");
            Assert.Equal(DocType.Xhtml10Strict, parser.DocType);
            Assert.Equal(ParsingMode.Xhtml, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Xhtml10Transitional + "<html></html>");
            Assert.Equal(DocType.Xhtml10Transitional, parser.DocType);
            Assert.Equal(ParsingMode.Xhtml, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Xhtml11 + "<html></html>");
            Assert.Equal(DocType.Xhtml11, parser.DocType);
            Assert.Equal(ParsingMode.Xhtml, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Xhtml20 + "<html></html>");
            Assert.Equal(DocType.Xhtml20, parser.DocType);
            Assert.Equal(ParsingMode.Xhtml, parser.ParsingMode);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_UnknownDocType() {
            var parser = new HtmlParser();

            parser.Parse("<!doctype randomname1><html></html>");
            Assert.Equal(DocType.Unrecognized, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse("<html></html>");
            Assert.Equal(DocType.Undefined, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_DuplicateDocType() {
            var parser = new HtmlParser();

            parser.Parse("<!doctype randomname1>" + DocTypeSignatures.Html5 + "<html></html>");
            Assert.Equal(DocType.Unrecognized, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);

            parser.Parse(DocTypeSignatures.Html5 + "<!doctype randomname1><html></html>");
            Assert.Equal(DocType.Html5, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_MalformedDocType() {
            var parser = new HtmlParser();

            parser.Parse("<!doctype randomname1<html></html>");
            Assert.Equal(DocType.Undefined, parser.DocType);
            Assert.Equal(ParsingMode.Html, parser.ParsingMode);
        }

        [Test]
        [Category.Html]
        public void HtmlParser_XmlPi() {
            var parser = new HtmlParser();

            parser.Parse("<?xml charset='utf-8' ?><html></html>");
            Assert.Equal(DocType.Undefined, parser.DocType);
            Assert.Equal(ParsingMode.Xml, parser.ParsingMode);
        }
    }
}

