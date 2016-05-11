// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class HtmlTokenizerTest {
        [Test]
        [Category.Html]
        public void HtmlTokenizer_GetNameToken_BasicTest() {
            var cs = new HtmlCharStream("foo");
            HtmlTokenizer target = new HtmlTokenizer(cs);
            NameToken actual = target.GetNameToken();

            Assert.Equal(3, actual.Length);
            Assert.Equal(0, actual.Start);
            Assert.Equal(3, actual.End);

            Assert.Equal(3, actual.NameRange.Length);
            Assert.Equal(0, actual.NameRange.Start);
            Assert.Equal(3, actual.NameRange.End);

            Assert.Equal(0, actual.PrefixRange.Start);
            Assert.Equal(0, actual.PrefixRange.End);
        }

        [Test]
        [Category.Html]
        public void HtmlTokenizer_GetNameToken_NamespaceTest() {
            var cs = new HtmlCharStream("foo:bar");
            HtmlTokenizer target = new HtmlTokenizer(cs);
            NameToken actual = target.GetNameToken();

            Assert.Equal(7, actual.Length);
            Assert.Equal(0, actual.Start);
            Assert.Equal(7, actual.End);

            Assert.Equal(3, actual.NameRange.Length);
            Assert.Equal(4, actual.NameRange.Start);
            Assert.Equal(7, actual.NameRange.End);

            Assert.Equal(0, actual.PrefixRange.Start);
            Assert.Equal(3, actual.PrefixRange.End);
            Assert.Equal(3, actual.PrefixRange.Length);
        }

        [Test]
        [Category.Html]
        public void HtmlTokenizer_GetNameToken_MissingPrefixTest() {
            var cs = new HtmlCharStream(":bar");
            HtmlTokenizer target = new HtmlTokenizer(cs);
            NameToken actual = target.GetNameToken();

            Assert.Equal(4, actual.Length);
            Assert.Equal(0, actual.Start);
            Assert.Equal(4, actual.End);

            Assert.False(actual.HasPrefix());
            Assert.Equal(0, actual.PrefixRange.Start);
            Assert.Equal(0, actual.PrefixRange.End);

            Assert.True(actual.HasName());
            Assert.Equal(1, actual.NameRange.Start);
            Assert.Equal(4, actual.NameRange.End);

            Assert.False(actual.HasQualifiedName());
            Assert.Equal(0, actual.QualifiedName.Start);
            Assert.Equal(4, actual.QualifiedName.End);
        }

        [Test]
        [Category.Html]
        public void HtmlTokenizer_GetNameToken_MissingNameTest() {
            var cs = new HtmlCharStream("foo:");
            HtmlTokenizer target = new HtmlTokenizer(cs);
            NameToken actual = target.GetNameToken();

            Assert.Equal(4, actual.Length);
            Assert.Equal(0, actual.Start);
            Assert.Equal(4, actual.End);

            Assert.True(actual.HasPrefix());
            Assert.Equal(0, actual.PrefixRange.Start);
            Assert.Equal(3, actual.PrefixRange.End);

            Assert.False(actual.HasName());
            Assert.Equal(0, actual.NameRange.Length);

            Assert.False(actual.HasQualifiedName());
            Assert.Equal(0, actual.QualifiedName.Start);
            Assert.Equal(4, actual.QualifiedName.End);
        }

        [Test]
        [Category.Html]
        public void HtmlTokenizer_SkipWhitespaceTest() {
            var cs = new HtmlCharStream("   abc\t\tdef\r\n gh");
            HtmlTokenizer target = new HtmlTokenizer(cs);
            target.SkipWhitespace();
            Assert.Equal(3, cs.Position);

            target.SkipWhitespace();
            Assert.Equal(3, cs.Position);

            cs.Advance(3);
            target.SkipWhitespace();
            Assert.Equal(8, cs.Position);

            cs.Advance(3);
            target.SkipWhitespace();
            Assert.Equal(14, cs.Position);
        }
    }
}
