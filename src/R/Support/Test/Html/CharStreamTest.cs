// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    public class CharStreamTest {
        [Test]
        [Category.Html]
        public void CharStream_BasicTest() {
            string text = "abcd\"foo\"\r\n<a href=";
            HtmlCharStream cs = new HtmlCharStream(text);

            Assert.Equal('a', cs.CurrentChar);

            cs.Advance(2);
            Assert.False(cs.IsEndOfStream());
            Assert.Equal('c', cs.CurrentChar);

            cs.Advance(-1);
            Assert.False(cs.IsEndOfStream());
            Assert.Equal('b', cs.CurrentChar);

            cs.Advance(text.Length);
            Assert.True(cs.IsEndOfStream());
            Assert.Equal(0, cs.CurrentChar);

            cs.Advance(-text.Length);
            Assert.False(cs.IsEndOfStream());
            Assert.Equal('a', cs.CurrentChar);

            Assert.Equal('d', cs.LookAhead(3));
            Assert.Equal('\"', cs.LookAhead(4));

            Assert.Equal(0, cs.LookAhead(text.Length));
            Assert.Equal(0, cs.LookAhead(-1));

            Assert.Equal(text.Length, cs.DistanceFromEnd);
            cs.Advance(1);
            Assert.Equal(text.Length - 1, cs.DistanceFromEnd);

            cs.Position = 4;
            Assert.True(cs.IsAtString());
            cs.Position = 5;
            Assert.False(cs.IsAtString());

            cs.Position = 9;
            Assert.True(cs.IsWhiteSpace());
            cs.MoveToNextChar();
            Assert.True(cs.IsWhiteSpace());

            cs.MoveToNextChar();
            Assert.True(cs.IsAtTagDelimiter());
        }
    }
}
