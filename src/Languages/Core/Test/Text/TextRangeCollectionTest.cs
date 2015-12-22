using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Tests.Text {
    public class TextRangeCollectionTest {
        private void AssertEquals(TextRangeCollection<TextRange> target, params int[] values) {
            Assert.Equal(target.Count, values.Length / 2);
            for (int i = 0; i < values.Length; i += 2) {
                Assert.Equal(values[i], target[i / 2].Start);
                Assert.Equal(values[i + 1], target[i / 2].End);
            }
        }

        TextRangeCollection<TextRange> MakeCollection() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            return new TextRangeCollection<TextRange>(ranges);
        }

        TextRangeCollection<TextRange> MakeCollection(params int[] positions) {
            TextRange[] ranges = new TextRange[positions.Length / 2];

            for (int i = 0; i < ranges.Length; i++) {
                int start = positions[2 * i];
                int end = positions[2 * i + 1];
                ranges[i] = TextRange.FromBounds(start, end);
            }

            return new TextRangeCollection<TextRange>(ranges);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ConstructorTest() {
            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            Assert.Equal(0, target.Count);
            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
            Assert.Equal(0, target.Length);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ConstructorTest1() {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.Equal(3, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
            Assert.Equal(6, target.Length);

            Assert.Equal(1, target[0].Start);
            Assert.Equal(3, target[1].Start);
            Assert.Equal(5, target[2].Start);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_AddTest() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            Assert.Equal(0, target.Count);

            target.Add(ranges[0]);
            Assert.Equal(1, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(2, target.End);
            Assert.Equal(1, target.Length);
            Assert.Equal(1, target[0].Start);

            target.Add(ranges[1]);
            Assert.Equal(2, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(5, target.End);
            Assert.Equal(4, target.Length);
            Assert.Equal(1, target[0].Start);
            Assert.Equal(3, target[1].Start);

            target.Add(ranges[2]);
            Assert.Equal(3, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
            Assert.Equal(6, target.Length);
            Assert.Equal(1, target[0].Start);
            Assert.Equal(3, target[1].Start);
            Assert.Equal(5, target[2].Start);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_AddTest1() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            target.Add(ranges);

            Assert.Equal(3, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
            Assert.Equal(6, target.Length);

            Assert.Equal(1, target[0].Start);
            Assert.Equal(3, target[1].Start);
            Assert.Equal(5, target[2].Start);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ClearTest() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            Assert.Equal(0, target.Count);
            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
            Assert.Equal(0, target.Length);

            target.Clear();

            Assert.Equal(0, target.Count);
            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
            Assert.Equal(0, target.Length);

            target.Add(ranges);

            Assert.Equal(3, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
            Assert.Equal(6, target.Length);

            target.Clear();

            Assert.Equal(0, target.Count);
            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
            Assert.Equal(0, target.Length);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ContainsTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.True(target.Contains(1));
            Assert.True(target.Contains(2));
            Assert.True(target.Contains(3));
            Assert.True(target.Contains(4));
            Assert.True(target.Contains(5));
            Assert.True(target.Contains(6));

            Assert.False(target.Contains(-10));
            Assert.False(target.Contains(0));
            Assert.False(target.Contains(7));
            Assert.False(target.Contains(Int32.MaxValue));
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_GetFirstItemAfterOrAtPositionTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.Equal(0, target.GetFirstItemAfterOrAtPosition(0));
            Assert.Equal(0, target.GetFirstItemAfterOrAtPosition(-2));

            Assert.Equal(0, target.GetFirstItemAfterOrAtPosition(1));
            Assert.Equal(1, target.GetFirstItemAfterOrAtPosition(2));

            Assert.Equal(1, target.GetFirstItemAfterOrAtPosition(3));
            Assert.Equal(1, target.GetFirstItemAfterOrAtPosition(4));
            Assert.Equal(2, target.GetFirstItemAfterOrAtPosition(5));

            Assert.Equal(-1, target.GetFirstItemAfterOrAtPosition(10));
            Assert.Equal(-1, target.GetFirstItemAfterOrAtPosition(Int32.MaxValue));
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_GetFirstItemBeforePositionTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            // 1-2, 3-5, 5-7


            Assert.Equal(-1, target.GetFirstItemBeforePosition(0));
            Assert.Equal(-1, target.GetFirstItemBeforePosition(-2));

            Assert.Equal(-1, target.GetFirstItemBeforePosition(1));

            Assert.Equal(0, target.GetFirstItemBeforePosition(2));
            Assert.Equal(0, target.GetFirstItemBeforePosition(3));
            Assert.Equal(0, target.GetFirstItemBeforePosition(4));

            Assert.Equal(1, target.GetFirstItemBeforePosition(5));
            Assert.Equal(1, target.GetFirstItemBeforePosition(6));

            Assert.Equal(2, target.GetFirstItemBeforePosition(7));
            Assert.Equal(2, target.GetFirstItemBeforePosition(8));
            Assert.Equal(2, target.GetFirstItemBeforePosition(9));
            Assert.Equal(2, target.GetFirstItemBeforePosition(10));
            Assert.Equal(2, target.GetFirstItemBeforePosition(Int32.MaxValue));
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_GetItemAtPositionTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.Equal(-1, target.GetItemAtPosition(0));
            Assert.Equal(-1, target.GetItemAtPosition(-2));

            Assert.Equal(0, target.GetItemAtPosition(1));
            Assert.Equal(-1, target.GetItemAtPosition(2));

            Assert.Equal(1, target.GetItemAtPosition(3));
            Assert.Equal(-1, target.GetItemAtPosition(4));
            Assert.Equal(2, target.GetItemAtPosition(5));

            Assert.Equal(-1, target.GetItemAtPosition(10));
            Assert.Equal(-1, target.GetItemAtPosition(Int32.MaxValue));
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_GetItemContainingTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.Equal(-1, target.GetItemContaining(0));
            Assert.Equal(-1, target.GetItemContaining(-2));

            Assert.Equal(0, target.GetItemContaining(1));
            Assert.Equal(-1, target.GetItemContaining(2));

            Assert.Equal(1, target.GetItemContaining(3));
            Assert.Equal(1, target.GetItemContaining(4));
            Assert.Equal(2, target.GetItemContaining(5));
            Assert.Equal(2, target.GetItemContaining(6));
            Assert.Equal(-1, target.GetItemContaining(7));

            Assert.Equal(-1, target.GetItemContaining(10));
            Assert.Equal(-1, target.GetItemContaining(Int32.MaxValue));
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_GetItemsContainingInclusiveEndTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            IReadOnlyList<int> list = target.GetItemsContainingInclusiveEnd(0);
            Assert.Equal(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(-2);
            Assert.Equal(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(1);
            Assert.Equal(1, list.Count);
            Assert.Equal(0, list[0]);

            list = target.GetItemsContainingInclusiveEnd(2);
            Assert.Equal(1, list.Count);
            Assert.Equal(0, list[0]);

            list = target.GetItemsContainingInclusiveEnd(3);
            Assert.Equal(1, list.Count);
            Assert.Equal(1, list[0]);

            list = target.GetItemsContainingInclusiveEnd(4);
            Assert.Equal(1, list.Count);
            Assert.Equal(1, list[0]);

            list = target.GetItemsContainingInclusiveEnd(5);
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);

            list = target.GetItemsContainingInclusiveEnd(6);
            Assert.Equal(1, list.Count);
            Assert.Equal(2, list[0]);

            list = target.GetItemsContainingInclusiveEnd(7);
            Assert.Equal(1, list.Count);
            Assert.Equal(2, list[0]);

            list = target.GetItemsContainingInclusiveEnd(8);
            Assert.Equal(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(10);
            Assert.Equal(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(Int32.MaxValue);
            Assert.Equal(0, list.Count);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ItemsInRangeTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            IReadOnlyList<TextRange> list = target.ItemsInRange(TextRange.EmptyRange);
            Assert.Equal(0, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(-10, -1));
            Assert.Equal(0, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(0, Int32.MaxValue));
            Assert.Equal(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2));
            Assert.Equal(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 7));
            Assert.Equal(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(0, 8));
            Assert.Equal(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 1));
            Assert.Equal(0, list.Count); // Zero-length ranges can't contain anything

            list = target.ItemsInRange(TextRange.FromBounds(1, 2));
            Assert.Equal(1, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 3));
            Assert.Equal(1, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 4));
            Assert.Equal(2, list.Count);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveAtTest() {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.RemoveAt(0);
            AssertEquals(target, 3, 5, 5, 7);

            target.RemoveAt(1);
            AssertEquals(target, 3, 5);

            target.RemoveAt(0);
            AssertEquals(target);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ShiftTest() {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.Shift(2);
            AssertEquals(target, 3, 4, 5, 7, 7, 9);

            target.Shift(-3);
            AssertEquals(target, 0, 1, 2, 4, 4, 6);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest1() {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            TextRange range = TextRange.FromBounds(1, 1);

            target.RemoveInRange(range);
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.RemoveInRange(TextRange.FromBounds(1, 2));
            AssertEquals(target, 3, 5, 5, 7);

            target.RemoveInRange(TextRange.FromBounds(5, 6));
            AssertEquals(target, 3, 5);

            Assert.Equal(3, target.Start);
            Assert.Equal(5, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest2() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2));
            Assert.Equal(0, target.Count);

            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest3() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(1, 7));

            Assert.Equal(0, target.Count);
            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest4() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(1, 6));
            Assert.Equal(0, target.Count);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest5() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 6));
            Assert.Equal(0, target.Count);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest6() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 5));
            AssertEquals(target, 5, 7);

            Assert.Equal(5, target.Start);
            Assert.Equal(7, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest7() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 1));
            Assert.Equal(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(2, 3));
            Assert.Equal(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(5, 5));
            Assert.Equal(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(7, 10));
            Assert.Equal(3, target.Count);

            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveInRangeTest8() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 0), inclusiveEnds: true);
            Assert.Equal(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(0, 1), inclusiveEnds: true);
            Assert.Equal(2, target.Count);

            target.RemoveInRange(TextRange.FromBounds(5, 5), inclusiveEnds: true);
            Assert.Equal(1, target.Count);

            Assert.Equal(3, target.Start);
            Assert.Equal(5, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ReflectTextChangeTest1() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(0, 0, 1);
            AssertEquals(target, 2, 3, 4, 6, 6, 8);

            target.ReflectTextChange(0, 1, 0);
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ReflectTextChangeTest2() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(3, 0, 3);
            AssertEquals(target, 1, 2, 6, 8, 8, 10);

            target.ReflectTextChange(8, 1, 0);
            AssertEquals(target, 1, 2, 6, 8, 8, 9);

            target.ReflectTextChange(8, 1, 0);
            AssertEquals(target, 1, 2, 6, 8);

            target.ReflectTextChange(7, 1, 0);
            AssertEquals(target, 1, 2, 6, 7);

            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ReflectTextChangeTest3() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(3, 1, 4);
            AssertEquals(target, 1, 2, 3, 8, 8, 10);

            target.ReflectTextChange(0, 15, 20);
            AssertEquals(target);

            Assert.Equal(0, target.Count);
            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ReflectTextChangeTest4() {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.ReflectTextChange(3, 0, 3);
            AssertEquals(target, 1, 2, 6, 8, 8, 10);

            Assert.Equal(1, target.Start);
            Assert.Equal(10, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_ShiftStartingFromTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ShiftStartingFrom(3, 4);
            AssertEquals(target, 1, 2, 3, 9, 9, 11);

            target.ShiftStartingFrom(1, -1);
            AssertEquals(target, 0, 1, 2, 8, 8, 10);

            target.ShiftStartingFrom(22, 10);
            AssertEquals(target, 0, 1, 2, 8, 8, 10);

            Assert.Equal(0, target.Start);
            Assert.Equal(10, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_RemoveLastItemZeroLength() {
            TextRangeCollection<TextRange> target;

            target = MakeCollection(1, 1);

            // testcase for deleting last range which was zero length
            target.ReflectTextChange(1, 1, 0);
            Assert.Equal(0, target.Count);

            Assert.Equal(0, target.Start);
            Assert.Equal(0, target.End);
        }

        [Fact]
        [Trait("Languages.Core", "")]
        public void TextRangeCollection_AddSorted() {
            ITextRange[] ranges = new ITextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<ITextRange> target = new TextRangeCollection<ITextRange>();

            Assert.Equal(0, target.Count);

            target.Add(ranges[2]);
            Assert.Equal(1, target.Count);

            target.AddSorted(ranges[0]);
            Assert.Equal(2, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
            Assert.Equal(1, target[0].Start);
            Assert.Equal(5, target[1].Start);

            target.AddSorted(ranges[1]);
            Assert.Equal(3, target.Count);
            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
            Assert.Equal(1, target[0].Start);
            Assert.Equal(3, target[1].Start);
            Assert.Equal(5, target[2].Start);

            Assert.Equal(1, target.Start);
            Assert.Equal(7, target.End);
        }
    }
}
