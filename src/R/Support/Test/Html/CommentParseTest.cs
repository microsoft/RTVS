// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser.States {
    [ExcludeFromCodeCoverage]
    public class CommentParseTest {
        [Test]
        [Category.Html]
        public void HtmlParser_OnCommentStateTest() {
            string text = "<!-- abcde -->";
            var target = new HtmlParser();
            target._cs = new HtmlCharStream(text);

            target._tokenizer = new HtmlTokenizer(target._cs);

            target.CommentFound +=
                delegate (object sender, HtmlParserCommentEventArgs args) {
                    Assert.True(args.CommentToken is CommentToken);
                    CommentToken ct = args.CommentToken;

                    Assert.Equal(1, ct.Count);

                    Assert.Equal(0, ct.Start);
                    Assert.Equal(14, ct.End);

                    Assert.True(ct[0] is HtmlToken);
                    Assert.True(ct[0] is IExpandableTextRange);

                    Assert.Equal(0, ct[0].Start);
                    Assert.Equal(14, ct[0].End);
                };

            target.OnCommentState();
        }
    }
}
