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
    public class EndTagParseTest {
        int endTagOpenCalls = 0;
        int endTagCloseCalls = 0;
        int attributeCalls = 0;

        [Test]
        [Category.Html]
        public void HtmlParser_OnEndTagStateTest() {
            var target = new HtmlParser();
            target._cs = new HtmlCharStream("</span \r\n foo='bar' nowrap>");

            target._tokenizer = new HtmlTokenizer(target._cs);
            target.EndTagOpen +=
                delegate (object sender, HtmlParserOpenTagEventArgs args) {
                    Assert.True(args.NameToken is NameToken);
                    Assert.Equal(2, args.NameToken.Start);
                    Assert.Equal(6, args.NameToken.End);
                    endTagOpenCalls++;
                };

            target.EndTagClose +=
                delegate (object sender, HtmlParserCloseTagEventArgs args) {
                    Assert.False(args.IsShorthand);
                    Assert.Equal(26, args.CloseAngleBracket.Start);
                    endTagCloseCalls++;
                };

            target.AttributeFound +=
                delegate (object sender, HtmlParserAttributeEventArgs args) {
                    attributeCalls++;
                };

            target.OnEndTagState();

            Assert.Equal(1, endTagOpenCalls);
            Assert.Equal(1, endTagCloseCalls);
            Assert.Equal(0, attributeCalls);
        }
    }
}
