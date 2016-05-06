// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Test.Utility;
using Microsoft.Html.Core.Tree;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser.States {
    [ExcludeFromCodeCoverage]
    public class AttributeParseTest {
        int attributeCalls = 0;

        [Test]
        [Category.Html]
        public void AttributeParse_WellFormed1() {
            var target = ParserAccessor.CreateParser("foo=bar");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(3, at.NameToken.End);

                    Assert.True(at.HasValue());
                    Assert.Equal(4, at.ValueToken.Start);
                    Assert.Equal(7, at.ValueToken.End);
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_WellFormed2() {
            var text = "class=\"foo\"";
            var target = ParserAccessor.CreateParser(text);

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(5, at.NameToken.End);

                    Assert.True(at.HasValue());
                    Assert.Equal(6, at.ValueToken.Start);
                    Assert.Equal(text.Length, at.ValueToken.End);
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_WellFormed3() {
            var text = "id=\"foo\"";
            var target = ParserAccessor.CreateParser(text);

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(2, at.NameToken.End);

                    Assert.True(at.HasValue());
                    Assert.Equal(3, at.ValueToken.Start);
                    Assert.Equal(text.Length, at.ValueToken.End);
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_WellFormed4() {
            var target = ParserAccessor.CreateParser("nowrap");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(6, at.NameToken.Length);

                    Assert.False(at.HasValue());
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_InlineStyle() {
            var target = ParserAccessor.CreateParser("style=\"display:none;\"");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(5, at.NameToken.Length);

                    Assert.True(at.HasValue());
                    Assert.Equal(6, at.ValueToken.Start);
                    Assert.Equal(15, at.ValueToken.Length);
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_InlineStyle_Typo() {
            var target = new HtmlParser();

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    switch (attributeCalls) {
                        case 0:
                            Assert.True(at.HasName());
                            Assert.Equal(7, at.NameToken.Start);
                            Assert.Equal(5, at.NameToken.Length);

                            Assert.True(at.HasValue());
                            Assert.Equal(13, at.ValueToken.Start);
                            Assert.Equal(14, at.ValueToken.Length);
                            break;

                        case 1:
                            Assert.True(at.HasName());
                            Assert.Equal(27, at.NameToken.Start);
                            Assert.Equal(1, at.NameToken.Length);

                            Assert.False(at.HasValue());
                            break;
                    }
                    attributeCalls++;
                };

            attributeCalls = 0;
            target.Parse("<input style=\"display:none\";>");
            Assert.Equal(2, attributeCalls);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_RandomCharacters() {
            var target = new HtmlParser();

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    switch (attributeCalls) {
                        case 0:
                            Assert.True(at.HasName());
                            Assert.Equal(3, at.NameToken.Start);
                            Assert.Equal(2, at.NameToken.Length);

                            Assert.True(at.HasValue());
                            Assert.Equal(6, at.ValueToken.Start);
                            Assert.Equal(1, at.ValueToken.Length);
                            break;

                        case 1:
                            Assert.True(at.HasName());
                            Assert.Equal(8, at.NameToken.Start);
                            Assert.Equal(1, at.NameToken.Length);

                            Assert.True(at.HasValue());
                            Assert.Equal(10, at.ValueToken.Start);
                            Assert.Equal(1, at.ValueToken.Length);
                            break;

                        case 2:
                            Assert.True(at.HasName());
                            Assert.Equal(12, at.NameToken.Start);
                            Assert.Equal(2, at.NameToken.Length);

                            Assert.False(at.HasValue());
                            break;
                    }
                    attributeCalls++;
                };

            attributeCalls = 0;
            target.Parse("<a id=# $=% ^^");

            Assert.Equal(3, attributeCalls);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_Script() {
            var text = "onclick=\"window.navigateTo(x<y)\"  ";
            var target = ParserAccessor.CreateParser(text);

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(7, at.NameToken.End);

                    Assert.True(at.HasValue());
                    Assert.Equal(8, at.ValueToken.Start);
                    Assert.Equal(text.Length - 2, at.ValueToken.End);
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_MissingValue() {
            var target = ParserAccessor.CreateParser("id=");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(2, at.NameToken.End);

                    Assert.False(at.HasValue());
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_MissingNameAndValue() {
            var target = ParserAccessor.CreateParser("=");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.False(at.HasName());
                    Assert.False(at.HasValue());
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParse_MissingName() {
            var target = ParserAccessor.CreateParser("=foo");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.False(at.HasName());
                    Assert.True(at.HasValue());
                    Assert.Equal(1, at.ValueToken.Start);
                    Assert.Equal(4, at.ValueToken.End);
                };

            target.OnAttributeState(100);
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_Namespaces() {
            var target = ParserAccessor.CreateParser("ns:name=foo");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());
                    Assert.Equal(0, at.NameToken.Start);
                    Assert.Equal(7, at.NameToken.End);

                    Assert.True(at.HasValue());
                    Assert.Equal(8, at.ValueToken.Start);
                    Assert.Equal(11, at.ValueToken.End);

                    NameToken nameToken = at.NameToken as NameToken;

                    Assert.True(nameToken.HasQualifiedName());

                    Assert.True(nameToken.NameRange.Length > 0);
                    Assert.Equal(3, nameToken.NameRange.Start);
                    Assert.Equal(7, nameToken.NameRange.End);

                    Assert.True(nameToken.PrefixRange.Length > 0);
                    Assert.Equal(0, nameToken.PrefixRange.Start);
                    Assert.Equal(2, nameToken.PrefixRange.End);
                };

            target.OnAttributeState();
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_NamespacesMissingName() {
            var target = ParserAccessor.CreateParser("ns:=foo");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.False(at.HasName());

                    Assert.True(at.HasValue());
                    Assert.Equal(4, at.ValueToken.Start);
                    Assert.Equal(7, at.ValueToken.End);

                    NameToken nameToken = at.NameToken as NameToken;

                    Assert.False(nameToken.HasName());
                    Assert.Equal(0, nameToken.NameRange.Length);

                    Assert.True(nameToken.HasPrefix());
                    Assert.Equal(0, nameToken.PrefixRange.Start);
                    Assert.Equal(2, nameToken.PrefixRange.End);
                };

            target.OnAttributeState();
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_NamespacesMissingPrefix() {
            var target = ParserAccessor.CreateParser(":name=foo");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.True(at.HasName());

                    NameToken nameToken = at.NameToken as NameToken;

                    Assert.False(nameToken.HasPrefix());
                    Assert.Equal(0, nameToken.PrefixRange.Length);

                    Assert.Equal(1, nameToken.ColonRange.Length);

                    Assert.True(nameToken.HasName());
                    Assert.Equal(1, nameToken.NameRange.Start);
                    Assert.Equal(4, nameToken.NameRange.Length);

                    Assert.False(nameToken.HasQualifiedName());

                    Assert.True(at.HasValue());
                    Assert.Equal(6, at.ValueToken.Start);
                    Assert.Equal(3, at.ValueToken.Length);
                };

            target.OnAttributeState();
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_NamespacesMissingNameAndPrefix() {
            var target = ParserAccessor.CreateParser(":=foo");

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    AttributeToken at = args.AttributeToken;

                    Assert.False(at.HasName());

                    Assert.True(at.HasValue());
                    Assert.Equal(2, at.ValueToken.Start);
                    Assert.Equal(3, at.ValueToken.Length);

                    NameToken nameToken = at.NameToken as NameToken;

                    Assert.Equal(1, nameToken.ColonRange.Length);

                    Assert.False(nameToken.HasName());
                    Assert.Equal(0, nameToken.NameRange.Length);

                    Assert.False(nameToken.HasPrefix());
                    Assert.Equal(0, nameToken.PrefixRange.Length);
                };
            target.OnAttributeState();
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_IncompleteTyping1() {
            var text = "<div lang=dir=ltr>";
            var tree = new HtmlTree(new TextStream(text));

            tree.Build();

            var div = tree.RootNode.Children[0];
            Assert.Equal(2, div.Attributes.Count);

            Assert.Equal("lang", div.Attributes[0].Name);
            Assert.False(div.Attributes[0].HasValue());

            Assert.Equal("dir", div.Attributes[1].Name);
            Assert.True(div.Attributes[1].HasValue());

            Assert.Equal("ltr", div.Attributes[1].Value);
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_IncompleteTyping2() {
            var text = "<div lang=dir=\"ltr\">";
            var tree = new HtmlTree(new TextStream(text));

            tree.Build();

            var div = tree.RootNode.Children[0];
            Assert.Equal(2, div.Attributes.Count);

            Assert.Equal("lang", div.Attributes[0].Name);
            Assert.False(div.Attributes[0].HasValue());

            Assert.Equal("dir", div.Attributes[1].Name);
            Assert.True(div.Attributes[1].HasValue());

            Assert.Equal("ltr", div.Attributes[1].Value);
            Assert.Equal('\"', div.Attributes[1].ValueToken.OpenQuote);
            Assert.Equal('\"', div.Attributes[1].ValueToken.CloseQuote);
        }

        [Test]
        [Category.Html]
        public void AttributeParsing_IncompleteTyping3() {
            var text = "<div lang=\"dir=\"ltr\">";
            var tree = new HtmlTree(new TextStream(text));

            tree.Build();

            var div = tree.RootNode.Children[0];
            Assert.Equal(2, div.Attributes.Count);

            Assert.Equal("lang", div.Attributes[0].Name);
            Assert.True(div.Attributes[0].HasValue());
            Assert.Equal(6, div.Attributes[0].ValueToken.Length);
            Assert.Equal('\"', div.Attributes[0].ValueToken.OpenQuote);
            Assert.Equal('\"', div.Attributes[0].ValueToken.CloseQuote);

            Assert.Equal("ltr\"", div.Attributes[1].Name);
            Assert.False(div.Attributes[1].HasValue());
        }
    }
}
