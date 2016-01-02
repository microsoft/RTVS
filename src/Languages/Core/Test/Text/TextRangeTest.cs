using System;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Tests.Text {
    public class TextRangeTest {
        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_IntersectTest01() {
            TextRange r1 = TextRange.FromBounds(1, 5);
            TextRange r2 = TextRange.FromBounds(5, 10);

            Assert.False(TextRange.Intersect(r1, r2));
            Assert.False(TextRange.Intersect(r2, r1));

            Assert.True(TextRange.Intersect(r1, r1));

            TextRange r3 = TextRange.FromBounds(1, 1);
            TextRange r4 = TextRange.FromBounds(1, 2);
            TextRange r5 = TextRange.FromBounds(2, 3);

            Assert.True(TextRange.Intersect(r1, r3));

            Assert.True(TextRange.Intersect(r1, r4));
            Assert.True(TextRange.Intersect(r1, r5));

            Assert.True(TextRange.Intersect(r3, r1));
            Assert.True(TextRange.Intersect(r4, r1));
            Assert.True(TextRange.Intersect(r5, r1));

            TextRange r6 = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            Assert.True(TextRange.Intersect(r1, r6));
            Assert.True(TextRange.Intersect(r6, r1));

            TextRange r7 = TextRange.FromBounds(0, 20);
            Assert.True(TextRange.Intersect(r1, r7));
            Assert.True(TextRange.Intersect(r7, r1));

            TextRange r8 = TextRange.FromBounds(5, 8);
            Assert.False(TextRange.Intersect(r1, r8));
            Assert.False(TextRange.Intersect(r8, r1));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_IntersectTest02() {
            TextRange r1 = TextRange.FromBounds(1, 5);
            TextRange r2 = TextRange.FromBounds(5, 10);

            Assert.False(r1.Intersect(r2));
            Assert.False(r2.Intersect(r1));

            Assert.True(r1.Intersect(r1));

            TextRange r3 = TextRange.FromBounds(1, 1);
            TextRange r4 = TextRange.FromBounds(1, 2);
            TextRange r5 = TextRange.FromBounds(2, 3);

            Assert.True(r1.Intersect(r3));

            Assert.True(r1.Intersect(r4));
            Assert.True(r1.Intersect(r5));

            Assert.True(r3.Intersect(r1));
            Assert.True(r4.Intersect(r1));
            Assert.True(r5.Intersect(r1));

            TextRange r6 = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            Assert.True(r1.Intersect(r6));
            Assert.True(r6.Intersect(r1));

            TextRange r7 = TextRange.FromBounds(0, 20);
            Assert.True(r1.Intersect(r7));
            Assert.True(r7.Intersect(r1));

            TextRange r8 = TextRange.FromBounds(5, 8);
            Assert.False(r1.Intersect(r8));
            Assert.False(r8.Intersect(r1));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_IsValidTest() {
            Assert.False(TextRange.IsValid(TextRange.EmptyRange));

            TextRange r1 = TextRange.FromBounds(1, 1);
            Assert.False(TextRange.IsValid(r1));

            TextRange r2 = TextRange.FromBounds(1, 2);
            Assert.True(TextRange.IsValid(r2));

            TextRange r4 = TextRange.FromBounds(2, 3);
            Assert.True(TextRange.IsValid(r4));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_CompareToTest() {
            TextRange r1 = TextRange.FromBounds(1, 1);

            Assert.False(r1.Equals(new object()));
            Assert.False(r1.Equals(null));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ConstructionTest1() {
            bool exception = false;

            try {
                TextRange t = TextRange.FromBounds(2, 1);
            } catch (Exception) {
                exception = true;
            }

            Assert.True(exception);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ConstructionTest2() {
            bool exception = false;

            try {
                TextRange t = TextRange.FromBounds(Int32.MinValue, Int32.MaxValue);
            } catch (Exception) {
                exception = true;
            }

            Assert.True(exception);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ConstructionTest3() {
            bool exception = false;

            try {
                TextRange t = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            } catch (Exception) {
                exception = true;
            }

            Assert.False(exception);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ConstructionTest4() {
            TextRange r = new TextRange(0);

            Assert.Equal(0, r.Start);
            Assert.Equal(1, r.Length);

            r = new TextRange();

            Assert.Equal(0, r.Start);
            Assert.Equal(1, r.Length);

            r = new TextRange(Int32.MaxValue);

            Assert.Equal(Int32.MaxValue, r.Start);
            Assert.Equal(0, r.Length);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ContainsTest() {
            TextRange r = TextRange.FromBounds(1, 3);

            Assert.False(TextRange.Contains(r, Int32.MinValue));
            Assert.False(TextRange.Contains(r, 0));

            Assert.True(TextRange.Contains(r, 1));
            Assert.True(TextRange.Contains(r, 2));

            Assert.False(TextRange.Contains(r, 3));
            Assert.False(TextRange.Contains(r, Int32.MaxValue));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ContainsTest1() {
            TextRange r = TextRange.FromBounds(1, 5);

            Assert.False(TextRange.Contains(r, TextRange.FromBounds(Int32.MinValue / 2, 0)));
            Assert.False(TextRange.Contains(r, TextRange.FromBounds(0, 1)));

            Assert.False(TextRange.Contains(r, TextRange.FromBounds(5, 6)));
            Assert.False(TextRange.Contains(r, TextRange.FromBounds(5, Int32.MaxValue / 2)));

            Assert.True(TextRange.Contains(r, TextRange.FromBounds(1, 2)));
            Assert.True(TextRange.Contains(r, TextRange.FromBounds(3, 4)));

            Assert.False(TextRange.Contains(r, TextRange.FromBounds(1, 5)));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_ContainsTest2() {
            TextRange r = TextRange.FromBounds(1, 5);

            Assert.False(r.Contains(TextRange.FromBounds(Int32.MinValue / 2, 0)));
            Assert.False(r.Contains(TextRange.FromBounds(0, 1)));

            Assert.False(r.Contains(TextRange.FromBounds(5, 6)));
            Assert.False(r.Contains(TextRange.FromBounds(5, Int32.MaxValue / 2)));

            Assert.True(r.Contains(TextRange.FromBounds(1, 2)));
            Assert.True(r.Contains(TextRange.FromBounds(3, 4)));

            Assert.False(r.Contains(TextRange.FromBounds(1, 5)));
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_EmptyTest() {
            TextRange r = TextRange.FromBounds(1, 2);
            r.Empty();

            Assert.False(TextRange.IsValid(r));
            Assert.Equal(0, r.Start);
            Assert.Equal(0, r.End);
            Assert.Equal(0, r.Length);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TextRange_AreEqualTest() {
            TextRange r = TextRange.FromBounds(1, 2);
            TextRange r1 = TextRange.FromBounds(1, 2);

            Assert.False(TextRange.Equals(r, TextRange.EmptyRange));
            Assert.True(TextRange.Equals(r, r));
            Assert.False(TextRange.Equals(r, null));
            Assert.False(TextRange.Equals(null, r));
        }
    }
}
