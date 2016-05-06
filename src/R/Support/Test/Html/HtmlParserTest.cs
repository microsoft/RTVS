// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class HtmlParserTest {
        [Test]
        [Category.Html]
        public void HtmlParser_FindEndOfBlockTest() {
            var target = new HtmlParser();
            string script = "<script>var x = \"boo\"; if(x < y) { }</script>";
            target._cs = new HtmlCharStream(script);
            target._tokenizer = new HtmlTokenizer(target._cs);

            var r = target.FindEndOfBlock("</script>");
            Assert.Equal(script.Length - 9, r.Length);
        }
    }
}
