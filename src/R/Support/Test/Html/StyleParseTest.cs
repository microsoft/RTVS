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
    public class StyleParseTest {
        [Test]
        [Category.Html]
        public void HtmlParser_OnStyleStateTest() {
            var target = new HtmlParser();
            target._cs = new HtmlCharStream(" body { background-color: green }</style>");
            target._tokenizer = new HtmlTokenizer(target._cs);
            int count = 0;

            target.StyleBlockFound +=
                delegate (object sender, HtmlParserBlockRangeEventArgs args) {
                    Assert.Equal(0, args.Range.Start);
                    Assert.Equal(33, args.Range.End);
                    count++;
                };

            target.OnStyleState(NameToken.Create(0, 0));
            Assert.Equal(1, count);
        }
    }
}
