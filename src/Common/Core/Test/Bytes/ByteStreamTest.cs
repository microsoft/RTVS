using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Languages.Core.Bytes;

namespace Microsoft.Languages.Core.Test.Bytes
{
    [TestClass]
    public class ByteStreamTest
    {
        [TestMethod]
        public void ByteStream_AdvanceTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            bool actual;
            actual = target.Advance(0);
            Assert.IsTrue(actual);
            Assert.AreEqual(0, target.Position);
            
            actual = target.Advance(1);
            Assert.IsTrue(actual);
            Assert.AreEqual(1, target.Position);

            actual = target.Advance(100);
            Assert.IsFalse(actual);
            Assert.AreEqual(5, target.Position);
        }

        [TestMethod]
        public void ByteStream_CurrentStringEqualsToTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            bool actual;
            actual = target.Advance(0);
            Assert.IsTrue(actual);
            Assert.AreEqual(0, target.Position);

            actual = target.Advance(1);
            Assert.IsTrue(actual);
            Assert.AreEqual(1, target.Position);

            actual = target.Advance(100);
            Assert.IsFalse(actual);
            Assert.AreEqual(5, target.Position);
        }

        [TestMethod]
        public void ByteStream_IsAnsiLetterTest()
        {
            byte[] text = new byte[256];
            int i = 0;

            for (i = 0; i < text.Length; i++)
                text[i] = (byte)i;

            ByteStream target = new ByteStream(text);

            for (i = 0; i < 'A'; i++)
            {
                Assert.IsFalse(target.IsAnsiLetter());
                target.MoveToNextChar();
            }

            for (; i <= 'z'; i++)
            {
                Assert.IsTrue(target.IsAnsiLetter());
                target.MoveToNextChar();
            }

            for (; i < text.Length; i++)
            {
                Assert.IsFalse(target.IsAnsiLetter());
                target.MoveToNextChar();
            }

            Assert.IsFalse(target.IsAnsiLetter());
        }

        [TestMethod]
        public void ByteStream_IsCharAtTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            for (int i = 0; i < text.Length; i++)
            {
                Assert.IsTrue(target.IsCharAt(i, text[i]));
            }
            
            Assert.IsFalse(target.IsCharAt(100, text[0]));
            Assert.AreEqual(0, target.Position);
        }

        [TestMethod]
        public void ByteStream_IsDigitTest()
        {
            byte[] text = new byte[256];

            int i;
            for (i = 0; i < text.Length; i++)
                text[i] = (byte)i;

            ByteStream target = new ByteStream(text);

            i = 0;
            for (; i < '0'; i++)
            {
                Assert.IsFalse(target.IsDigit());
                target.MoveToNextChar();
            }

            for (; i <= '9'; i++)
            {
                Assert.IsTrue(target.IsDigit());
                target.MoveToNextChar();
            }

            for (; i < text.Length; i++)
            {
                Assert.IsFalse(target.IsDigit());
                target.MoveToNextChar();
            }

            Assert.IsFalse(target.IsDigit());
        }

        [TestMethod]
        public void ByteStream_IsEndOfStreamTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.IsFalse(target.IsEndOfStream());

            target.Advance(1);
            Assert.IsFalse(target.IsEndOfStream());

            target.Advance(100);
            Assert.IsTrue(target.IsEndOfStream());

            text = new byte[0];
            target = new ByteStream(text);
            Assert.IsTrue(target.IsEndOfStream());
        }

        [TestMethod]
        public void ByteStream_IsNewLineCharTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'\r', (byte)'\n', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);

            Assert.IsFalse(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.IsTrue(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.IsTrue(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.IsFalse(target.IsNewLineChar());
            target.MoveToNextChar();
            Assert.IsFalse(target.IsNewLineChar());
        }

        [TestMethod]
        public void ByteStream_IsWhiteSpaceTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'\r', (byte)'\n', (byte)'\t', (byte)' ', };
            ByteStream target = new ByteStream(text);

            Assert.IsFalse(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.IsTrue(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.IsTrue(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.IsTrue(target.IsWhiteSpace());
            target.MoveToNextChar();
            Assert.IsTrue(target.IsWhiteSpace());
        }

        [TestMethod]
        public void ByteStream_CurrentCharTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.AreEqual((byte)'a', target.CurrentChar);

            target.Advance(1);
            Assert.AreEqual((byte)'b', target.CurrentChar);

            target.Advance(100);
            Assert.AreEqual((byte)0, target.CurrentChar);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.AreEqual((byte)0, target.CurrentChar);
        }

        [TestMethod]
        public void ByteStream_DistanceFromEndTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.AreEqual(5, target.DistanceFromEnd);

            target.Advance(1);
            Assert.AreEqual(4, target.DistanceFromEnd);

            target.Advance(100);
            Assert.AreEqual(0, target.DistanceFromEnd);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.AreEqual(0, target.DistanceFromEnd);
        }

        [TestMethod]
        public void ByteStream_LengthTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.AreEqual(5, target.Length);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.AreEqual(0, target.Length);
        }

        [TestMethod]
        public void ByteStream_NextCharTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);

            for (int i = 0; i < text.Length - 1; i++)
            {
                Assert.AreEqual(text[i + 1], target.NextChar);
                target.MoveToNextChar();
            }

            Assert.AreEqual((byte)0, target.NextChar);
        }

        [TestMethod]
        public void ByteStream_PositionTest()
        {
            byte[] text = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', };
            ByteStream target = new ByteStream(text);
            Assert.AreEqual(0, target.Position);

            target.Advance(1);
            Assert.AreEqual(1, target.Position);

            target.Advance(100);
            Assert.AreEqual(5, target.Position);

            text = new byte[0];
            target = new ByteStream(text);
            Assert.AreEqual(0, target.Position);
        }
    }
}
