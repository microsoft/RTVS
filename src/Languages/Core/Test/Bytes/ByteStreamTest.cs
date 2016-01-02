using Microsoft.Languages.Core.Bytes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Tests.Bytes {
    public class ByteStreamTest {
        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_AdvanceTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            bool actual;
            actual = target.Advance(0);
            Assert.True(actual);
            Assert.Equal(0, target.Position);

            actual = target.Advance(1);
            Assert.True(actual);
            Assert.Equal(1, target.Position);

            actual = target.Advance(100);
            Assert.False(actual);
            Assert.Equal(5, target.Position);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_CurrentStringEqualsToTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            bool actual;
            actual = target.Advance(0);
            Assert.True(actual);
            Assert.Equal(0, target.Position);

            actual = target.Advance(1);
            Assert.True(actual);
            Assert.Equal(1, target.Position);

            actual = target.Advance(100);
            Assert.False(actual);
            Assert.Equal(5, target.Position);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_IsAnsiLetterTest() {
            byte[] text = new byte[256];
            int i = 0;

            for (i = 0; i < text.Length; i++)
                text[i] = (byte)i;

            ByteStream target = new ByteStream(text);

            for (i = 0; i < 'A'; i++) {
                Assert.False(target.IsAnsiLetter());
                target.MoveToNextChar();
            }

            for (; i <= 'z'; i++) {
                Assert.True(target.IsAnsiLetter());
                target.MoveToNextChar();
            }

            for (; i < text.Length; i++) {
                Assert.False(target.IsAnsiLetter());
                target.MoveToNextChar();
            }

            Assert.False(target.IsAnsiLetter());
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_IsCharAtTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            for (int i = 0; i < text.Length; i++) {
                Assert.True(target.IsCharAt(i, text[i]));
            }

            Assert.False(target.IsCharAt(100, text[0]));
            Assert.Equal(0, target.Position);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_IsDigitTest() {
            byte[] text = new byte[256];

            int i;
            for (i = 0; i < text.Length; i++)
                text[i] = (byte)i;

            ByteStream target = new ByteStream(text);

            i = 0;
            for (; i < '0'; i++) {
                Assert.False(target.IsDigit());
                target.MoveToNextChar();
            }

            for (; i <= '9'; i++) {
                Assert.True(target.IsDigit());
                target.MoveToNextChar();
            }

            for (; i < text.Length; i++) {
                Assert.False(target.IsDigit());
                target.MoveToNextChar();
            }

            Assert.False(target.IsDigit());
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_IsEndOfStreamTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.False(target.IsEndOfStream());

            target.Advance(1);
            Assert.False(target.IsEndOfStream());

            target.Advance(100);
            Assert.True(target.IsEndOfStream());

            text = new byte[0];
            target = new ByteStream(text);
            Assert.True(target.IsEndOfStream());
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_IsNewLineCharTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'\r', (byte)'\n', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);

            Assert.False(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.True(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.True(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.False(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.False(target.IsNewLineChar());
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_IsWhiteSpaceTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'\r', (byte)'\n', (byte)'\t', (byte)' ', };
            ByteStream target = new ByteStream(text);

            Assert.False(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.True(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.True(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.True(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.True(target.IsWhiteSpace());
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_CurrentCharTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.Equal((byte)'a', target.CurrentChar);

            target.Advance(1);
            Assert.Equal((byte)'b', target.CurrentChar);

            target.Advance(100);
            Assert.Equal((byte)0, target.CurrentChar);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.Equal((byte)0, target.CurrentChar);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_DistanceFromEndTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.Equal(5, target.DistanceFromEnd);

            target.Advance(1);
            Assert.Equal(4, target.DistanceFromEnd);

            target.Advance(100);
            Assert.Equal(0, target.DistanceFromEnd);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.Equal(0, target.DistanceFromEnd);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_LengthTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.Equal(5, target.Length);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.Equal(0, target.Length);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_NextCharTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);

            for (int i = 0; i < text.Length - 1; i++) {
                Assert.Equal(text[i + 1], target.NextChar);
                target.MoveToNextChar();
            }

            Assert.Equal((byte)0, target.NextChar);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_PositionTest() {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.Equal(0, target.Position);

            target.Advance(1);
            Assert.Equal(1, target.Position);

            target.Advance(100);
            Assert.Equal(5, target.Position);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.Equal(0, target.Position);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void ByteStream_StringEqualsTest() {
            ByteStream bs = new ByteStream(new byte[0]);
            Assert.True(bs.CurrentStringEqualsTo("", 0));
            Assert.True(bs.CurrentStringEqualsTo("a", 0));
            Assert.False(bs.CurrentStringEqualsTo("abc", 3));

            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            bs = new ByteStream(text);

            Assert.True(bs.CurrentStringEqualsTo("abcd", 4));
            Assert.True(bs.CurrentStringEqualsTo("abcdef", 5));
            Assert.False(bs.CurrentStringEqualsTo("abcdef", 6));
            Assert.True(bs.CurrentStringEqualsTo("", 0));
            Assert.False(bs.CurrentStringEqualsTo("", 1));
        }
    }
}
