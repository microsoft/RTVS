// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Bytes;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Bytes {
    [ExcludeFromCodeCoverage]
    [Category.Languages.Core]
    public class ByteStreamTest {
        [Test]
        public void ByteStream_AdvanceTest() {
            byte[] text = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            var actual = target.Advance(0);
            actual.Should().BeTrue();
            target.Position.Should().Be(0);

            actual = target.Advance(1);
            actual.Should().BeTrue();
            target.Position.Should().Be(1);

            actual = target.Advance(100);
            actual.Should().BeFalse();
            target.Position.Should().Be(5);
        }

        [Test]
        public void ByteStream_CurrentStringEqualsToTest() {
            byte[] text = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);

            var actual = target.Advance(0);
            actual.Should().BeTrue();
            target.Position.Should().Be(0);

            actual = target.Advance(1);
            actual.Should().BeTrue();
            target.Position.Should().Be(1);

            actual = target.Advance(100);
            actual.Should().BeFalse();
            target.Position.Should().Be(5);
        }

        [Test]
        public void ByteStream_IsAnsiLetterTest() {
            var text = new byte[256];
            int i;

            for (i = 0; i < text.Length; i++) {
                text[i] = (byte)i;
            }

            var target = new ByteStream(text);

            for (i = 0; i < 'A'; i++) {
                target.IsAnsiLetter().Should().BeFalse();
                target.MoveToNextChar();
            }

            for (; i <= 'z'; i++) {
                target.IsAnsiLetter().Should().BeTrue();
                target.MoveToNextChar();
            }

            for (; i < text.Length; i++) {
                target.IsAnsiLetter().Should().BeFalse();
                target.MoveToNextChar();
            }

            target.IsAnsiLetter().Should().BeFalse();
        }

        [Test]
        public void ByteStream_IsCharAtTest() {
            var text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            for (var i = 0; i < text.Length; i++) {
                target.IsCharAt(i, text[i]).Should().BeTrue();
            }

            target.IsCharAt(100, text[0]).Should().BeFalse();
            target.Position.Should().Be(0);
        }

        [Test]
        public void ByteStream_IsDigitTest() {
            var text = new byte[256];

            int i;
            for (i = 0; i < text.Length; i++) {
                text[i] = (byte)i;
            }

            var target = new ByteStream(text);

            i = 0;
            for (; i < '0'; i++) {
                target.IsDigit().Should().BeFalse();
                target.MoveToNextChar();
            }

            for (; i <= '9'; i++) {
                target.IsDigit().Should().BeTrue();
                target.MoveToNextChar();
            }

            for (; i < text.Length; i++) {
                target.IsDigit().Should().BeFalse();
                target.MoveToNextChar();
            }

            target.IsDigit().Should().BeFalse();
        }

        [Test]
        public void ByteStream_IsEndOfStreamTest() {
            var text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            target.IsEndOfStream().Should().BeFalse();

            target.Advance(1);
            target.IsEndOfStream().Should().BeFalse();

            target.Advance(100);
            target.IsEndOfStream().Should().BeTrue();

            text = new byte[0];
            target = new ByteStream(text);
            target.IsEndOfStream().Should().BeTrue();
        }

        [Test]
        public void ByteStream_IsNewLineCharTest() {
            var text = new byte[] { (byte)'a', (byte)'\r', (byte)'\n', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);

            target.IsNewLineChar().Should().BeFalse();
            target.MoveToNextChar();
            target.IsNewLineChar().Should().BeTrue();
            target.MoveToNextChar();
            target.IsNewLineChar().Should().BeTrue();
            target.MoveToNextChar();
            target.IsNewLineChar().Should().BeFalse();
            target.MoveToNextChar();
            target.IsNewLineChar().Should().BeFalse();
        }

        [Test]
        public void ByteStream_IsWhiteSpaceTest() {
            var text = new byte[] { (byte)'a', (byte)'\r', (byte)'\n', (byte)'\t', (byte)' ', };
            var target = new ByteStream(text);

            target.IsWhiteSpace().Should().BeFalse();
            target.MoveToNextChar();
            target.IsWhiteSpace().Should().BeTrue();
            target.MoveToNextChar();
            target.IsWhiteSpace().Should().BeTrue();
            target.MoveToNextChar();
            target.IsWhiteSpace().Should().BeTrue();
            target.MoveToNextChar();
            target.IsWhiteSpace().Should().BeTrue();
        }

        [Test]
        public void ByteStream_CurrentCharTest() {
            var text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            target.CurrentChar.Should().Be((byte)'a');

            target.Advance(1);
            target.CurrentChar.Should().Be((byte)'b');

            target.Advance(100);
            target.CurrentChar.Should().Be(0);

            text = new byte[0];
            target = new ByteStream(text);
            target.CurrentChar.Should().Be(0);
        }

        [Test]
        public void ByteStream_DistanceFromEndTest() {
            byte[] text = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            target.DistanceFromEnd.Should().Be(5);

            target.Advance(1);
            target.DistanceFromEnd.Should().Be(4);

            target.Advance(100);
            target.DistanceFromEnd.Should().Be(0);

            text = new byte[0];
            target = new ByteStream(text);
            target.DistanceFromEnd.Should().Be(0);
        }

        [Test]
        public void ByteStream_LengthTest() {
            byte[] text = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            target.Length.Should().Be(5);

            text = new byte[0];
            target = new ByteStream(text);
            target.Length.Should().Be(0);
        }

        [Test]
        public void ByteStream_NextCharTest() {
            byte[] text = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);

            for (var i = 0; i < text.Length - 1; i++) {
                target.NextChar.Should().Be(text[i + 1]);
                target.MoveToNextChar();
            }

            target.NextChar.Should().Be(0);
        }

        [Test]
        public void ByteStream_PositionTest() {
            var text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            var target = new ByteStream(text);
            target.Position.Should().Be(0);

            target.Advance(1);
            target.Position.Should().Be(1);

            target.Advance(100);
            target.Position.Should().Be(5);

            text = new byte[0];
            target = new ByteStream(text);
            target.Position.Should().Be(0);
        }

        [Test]
        public void ByteStream_StringEqualsTest() {
            var bs = new ByteStream(new byte[0]);
            bs.CurrentStringEqualsTo("", 0).Should().BeTrue();
            bs.CurrentStringEqualsTo("a", 0).Should().BeTrue();
            bs.CurrentStringEqualsTo("abc", 3).Should().BeFalse();

            byte[] text = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            bs = new ByteStream(text);

            bs.CurrentStringEqualsTo("abcd", 4).Should().BeTrue();
            bs.CurrentStringEqualsTo("abcdef", 5).Should().BeTrue();
            bs.CurrentStringEqualsTo("abcdef", 6).Should().BeFalse();
            bs.CurrentStringEqualsTo("", 0).Should().BeTrue();
            bs.CurrentStringEqualsTo("", 1).Should().BeFalse();
        }
    }
}
