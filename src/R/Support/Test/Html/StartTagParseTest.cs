// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser.States {
    [ExcludeFromCodeCoverage]
    public class StartTagParseTest {
        [Test]
        [Category.Html]
        public void OnStartTagState_WellFormedTest() {
            var target = new HtmlParser();
            target._cs = new HtmlCharStream("<input\r\n type='button' nowrap/>");

            int startTagOpenCalls = 0;
            int startTagCloseCalls = 0;
            int attributeCalls = 0;

            target._tokenizer = new HtmlTokenizer(target._cs);
            target.StartTagOpen +=
                delegate (object sender, HtmlParserOpenTagEventArgs args) {
                    Assert.True(args.NameToken is NameToken);
                    Assert.Equal(1, args.NameToken.Start);
                    Assert.Equal(6, args.NameToken.End);
                    startTagOpenCalls++;
                };

            target.StartTagClose +=
                delegate (object sender, HtmlParserCloseTagEventArgs args) {
                    Assert.Equal(29, args.CloseAngleBracket.Start);
                    Assert.Equal(31, args.CloseAngleBracket.End);
                    Assert.True(args.IsShorthand);
                    Assert.True(args.IsClosed);
                    startTagCloseCalls++;
                };

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    var at = args.AttributeToken;

                    switch (attributeCalls) {
                        case 0:
                            Assert.True(at.HasName());
                            Assert.True(at.HasValue());
                            Assert.Equal(9, at.NameToken.Start);
                            Assert.Equal(13, at.NameToken.End);
                            Assert.Equal(14, at.ValueToken.Start);
                            Assert.Equal(22, at.ValueToken.End);
                            break;

                        case 1:
                            Assert.True(at.HasName());
                            Assert.False(at.HasValue());
                            Assert.Equal(23, at.NameToken.Start);
                            Assert.Equal(29, at.NameToken.End);
                            break;
                    }
                    attributeCalls++;
                };

            target.OnStartTagState();

            Assert.Equal(1, startTagOpenCalls);
            Assert.Equal(1, startTagCloseCalls);
            Assert.Equal(2, attributeCalls);
        }

        [Test]
        [Category.Html]
        public void OnStartTagStateTest_UnClosed() {
            var target = new HtmlParser();
            target._cs = new HtmlCharStream("<input\r\n type='button' nowrap <table>");

            int startTagOpenCalls = 0;
            int startTagCloseCalls = 0;
            int attributeCalls = 0;

            target._tokenizer = new HtmlTokenizer(target._cs);
            target.StartTagOpen +=
                delegate (object sender, HtmlParserOpenTagEventArgs args) {
                    Assert.True(args.NameToken is NameToken);
                    Assert.Equal(1, args.NameToken.Start);
                    Assert.Equal(6, args.NameToken.End);
                    startTagOpenCalls++;
                };

            target.StartTagClose +=
                delegate (object sender, HtmlParserCloseTagEventArgs args) {
                    Assert.Equal(30, args.CloseAngleBracket.Start);
                    Assert.False(args.IsClosed);
                    Assert.False(args.IsShorthand);
                    startTagCloseCalls++;
                };

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    Assert.True(args.AttributeToken is AttributeToken);
                    var at = args.AttributeToken;

                    switch (attributeCalls) {
                        case 0:
                            Assert.True(at.HasName());
                            Assert.True(at.HasValue());
                            Assert.Equal(9, at.NameToken.Start);
                            Assert.Equal(13, at.NameToken.End);
                            Assert.Equal(14, at.ValueToken.Start);
                            Assert.Equal(22, at.ValueToken.End);
                            break;

                        case 1:
                            Assert.True(at.HasName());
                            Assert.False(at.HasValue());
                            Assert.Equal(23, at.NameToken.Start);
                            Assert.Equal(29, at.NameToken.End);
                            break;
                    }
                    attributeCalls++;
                };

            target.OnStartTagState();

            Assert.Equal(1, startTagOpenCalls);
            Assert.Equal(1, startTagCloseCalls);
            Assert.Equal(2, attributeCalls);
        }
    }
}
