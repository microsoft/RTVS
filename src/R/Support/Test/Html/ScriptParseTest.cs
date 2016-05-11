// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser.States {
    [ExcludeFromCodeCoverage]
    public class ScriptParseTest {
        [Test]
        [Category.Html]
        public void HtmlParser_OnScriptStateTest() {
            string text = " var x = 1; while(x < 5 && x > 0) { x++; }</script>";
            var target = new HtmlParser();
            target._cs = new HtmlCharStream(text);
            target._tokenizer = new HtmlTokenizer(target._cs);

            int count = 0;
            target.ScriptBlockFound +=
                    delegate (object sender, HtmlParserBlockRangeEventArgs args) {
                        Assert.Equal(0, args.Range.Start);
                        Assert.Equal(text.Length - 9, args.Range.End);
                        count++;
                    };

            target.OnScriptState(String.Empty, NameToken.Create(0, 0));
            Assert.Equal(1, count);
        }
    }
}
