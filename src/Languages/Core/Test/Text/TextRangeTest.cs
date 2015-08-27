using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Text
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TextRangeTest
    {
        [TestMethod]
        public void TextRange_IntersectTest01()
        {
            TextRange r1 = TextRange.FromBounds(1, 5);
            TextRange r2 = TextRange.FromBounds(5, 10);

            Assert.IsFalse(TextRange.Intersect(r1, r2));
            Assert.IsFalse(TextRange.Intersect(r2, r1));

            Assert.IsTrue(TextRange.Intersect(r1, r1));

            TextRange r3 = TextRange.FromBounds(1, 1);
            TextRange r4 = TextRange.FromBounds(1, 2);
            TextRange r5 = TextRange.FromBounds(2, 3);

            Assert.IsTrue(TextRange.Intersect(r1, r3));

            Assert.IsTrue(TextRange.Intersect(r1, r4));
            Assert.IsTrue(TextRange.Intersect(r1, r5));

            Assert.IsTrue(TextRange.Intersect(r3, r1));
            Assert.IsTrue(TextRange.Intersect(r4, r1));
            Assert.IsTrue(TextRange.Intersect(r5, r1));

            TextRange r6 = TextRange.FromBounds(Int32.MinValue/2, Int32.MaxValue/2);
            Assert.IsTrue(TextRange.Intersect(r1, r6));
            Assert.IsTrue(TextRange.Intersect(r6, r1));

            TextRange r7 = TextRange.FromBounds(0, 20);
            Assert.IsTrue(TextRange.Intersect(r1, r7));
            Assert.IsTrue(TextRange.Intersect(r7, r1));

            TextRange r8 = TextRange.FromBounds(5, 8);
            Assert.IsFalse(TextRange.Intersect(r1, r8));
            Assert.IsFalse(TextRange.Intersect(r8, r1));
        }

        [TestMethod]
        public void TextRange_IntersectTest02()
        {
            TextRange r1 = TextRange.FromBounds(1, 5);
            TextRange r2 = TextRange.FromBounds(5, 10);

            Assert.IsFalse(r1.Intersect(r2));
            Assert.IsFalse(r2.Intersect(r1));

            Assert.IsTrue(r1.Intersect(r1));

            TextRange r3 = TextRange.FromBounds(1, 1);
            TextRange r4 = TextRange.FromBounds(1, 2);
            TextRange r5 = TextRange.FromBounds(2, 3);

            Assert.IsTrue(r1.Intersect(r3));

            Assert.IsTrue(r1.Intersect(r4));
            Assert.IsTrue(r1.Intersect(r5));

            Assert.IsTrue(r3.Intersect(r1));
            Assert.IsTrue(r4.Intersect(r1));
            Assert.IsTrue(r5.Intersect(r1));

            TextRange r6 = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            Assert.IsTrue(r1.Intersect(r6));
            Assert.IsTrue(r6.Intersect(r1));

            TextRange r7 = TextRange.FromBounds(0, 20);
            Assert.IsTrue(r1.Intersect(r7));
            Assert.IsTrue(r7.Intersect(r1));

            TextRange r8 = TextRange.FromBounds(5, 8);
            Assert.IsFalse(r1.Intersect(r8));
            Assert.IsFalse(r8.Intersect(r1));
        }

        [TestMethod]
        public void TextRange_IsValidTest()
        {
            Assert.IsFalse(TextRange.IsValid(TextRange.EmptyRange));

            TextRange r1 = TextRange.FromBounds(1, 1);
            Assert.IsFalse(TextRange.IsValid(r1));

            TextRange r2 = TextRange.FromBounds(1, 2);
            Assert.IsTrue(TextRange.IsValid(r2));

            TextRange r4 = TextRange.FromBounds(2, 3);
            Assert.IsTrue(TextRange.IsValid(r4));
        }

        [TestMethod]
        public void TextRange_CompareToTest()
        {
            TextRange r1 = TextRange.FromBounds(1, 1);

            Assert.IsFalse(r1.Equals(new object()));
            Assert.IsFalse(r1.Equals(null));
        }

        [TestMethod]
        public void TextRange_ConstructionTest1()
        {
            bool exception = false;

            try
            {
                 TextRange t = TextRange.FromBounds(2, 1);
            }
            catch (Exception)
            {
                exception = true;
            }

            Assert.IsTrue(exception);
        }

        [TestMethod]
        public void TextRange_ConstructionTest2()
        {
            bool exception = false;

            try
            {
                TextRange t = TextRange.FromBounds(Int32.MinValue, Int32.MaxValue);
            }
            catch (Exception)
            {
                exception = true;
            }

            Assert.IsTrue(exception);
        }

        [TestMethod]
        public void TextRange_ConstructionTest3()
        {
            bool exception = false;

            try
            {
                TextRange t = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            }
            catch (Exception)
            {
                exception = true;
            }

            Assert.IsFalse(exception);
        }

        [TestMethod]
        public void TextRange_ConstructionTest4()
        {
            TextRange r = new TextRange(0);

            Assert.AreEqual(0, r.Start);
            Assert.AreEqual(1, r.Length);

            r = new TextRange();

            Assert.AreEqual(0, r.Start);
            Assert.AreEqual(1, r.Length);

            r = new TextRange(Int32.MaxValue);

            Assert.AreEqual(Int32.MaxValue, r.Start);
            Assert.AreEqual(0, r.Length);
        }

        [TestMethod]
        public void TextRange_ContainsTest()
        {
            TextRange r = TextRange.FromBounds(1, 3);

            Assert.IsFalse(TextRange.Contains(r, Int32.MinValue));
            Assert.IsFalse(TextRange.Contains(r, 0));
            
            Assert.IsTrue(TextRange.Contains(r, 1));
            Assert.IsTrue(TextRange.Contains(r, 2));

            Assert.IsFalse(TextRange.Contains(r, 3));
            Assert.IsFalse(TextRange.Contains(r, Int32.MaxValue));
        }

        [TestMethod]
        public void TextRange_ContainsTest1()
        {
            TextRange r = TextRange.FromBounds(1, 5);

            Assert.IsFalse(TextRange.Contains(r, TextRange.FromBounds(Int32.MinValue/2, 0)));
            Assert.IsFalse(TextRange.Contains(r, TextRange.FromBounds(0, 1)));
            
            Assert.IsFalse(TextRange.Contains(r, TextRange.FromBounds(5, 6)));
            Assert.IsFalse(TextRange.Contains(r, TextRange.FromBounds(5, Int32.MaxValue/2)));

            Assert.IsTrue(TextRange.Contains(r, TextRange.FromBounds(1, 2)));
            Assert.IsTrue(TextRange.Contains(r, TextRange.FromBounds(3, 4)));

            Assert.IsFalse(TextRange.Contains(r, TextRange.FromBounds(1, 5)));
        }

        [TestMethod]
        public void TextRange_ContainsTest2()
        {
            TextRange r = TextRange.FromBounds(1, 5);

            Assert.IsFalse(r.Contains(TextRange.FromBounds(Int32.MinValue / 2, 0)));
            Assert.IsFalse(r.Contains(TextRange.FromBounds(0, 1)));

            Assert.IsFalse(r.Contains(TextRange.FromBounds(5, 6)));
            Assert.IsFalse(r.Contains(TextRange.FromBounds(5, Int32.MaxValue / 2)));

            Assert.IsTrue(r.Contains(TextRange.FromBounds(1, 2)));
            Assert.IsTrue(r.Contains(TextRange.FromBounds(3, 4)));

            Assert.IsFalse(r.Contains(TextRange.FromBounds(1, 5)));
        }

        [TestMethod]
        public void TextRange_EmptyTest()
        {
            TextRange r = TextRange.FromBounds(1, 2);
            r.Empty();
            
            Assert.IsFalse(TextRange.IsValid(r));
            Assert.AreEqual(0, r.Start);
            Assert.AreEqual(0, r.End);
            Assert.AreEqual(0, r.Length);
        }

        [TestMethod]
        public void TextRange_AreEqualTest()
        {
            TextRange r = TextRange.FromBounds(1, 2);
            TextRange r1 = TextRange.FromBounds(1, 2);

            Assert.IsFalse(TextRange.AreEqual(r, TextRange.EmptyRange));
            Assert.IsTrue(TextRange.AreEqual(r, r));
            Assert.IsFalse(TextRange.AreEqual(r, null));
            Assert.IsFalse(TextRange.AreEqual(null, r));
            Assert.IsTrue(TextRange.AreEqual(r, r1));
        }
    }
}
