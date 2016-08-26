// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.RD.Parser;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Support.Test.Functions {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class PlainTextExtractorTest {
        [CompositeTest]
        [InlineData("", "")]
        [InlineData(" a b c ", " a b c ")]
        [InlineData("<b>a </b>", "a ")]
        [InlineData("a <b>b </b>", "a b ")]
        [InlineData("a<b> b </b>c", "a b c")]
        [InlineData("a<b> b c", "a b c")]
        [InlineData("a</b> b c", "a b c")]
        public void TextFromHtml(string html, string expected) {
            (new PlainTextExtractor()).GetTextFromHtml(html).Should().Be(expected);
        }
    }
}
