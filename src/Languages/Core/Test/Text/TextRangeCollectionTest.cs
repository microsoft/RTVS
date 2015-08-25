using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Text
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TextRangeCollectionTest
    {
        TextRangeCollection<TextRange> MakeInclusiveCollection()
        {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = new InclusiveTextRange(1, 3, true, false, false);
            ranges[1] = new InclusiveTextRange(4, 3, true, true, true);
            ranges[2] = new InclusiveTextRange(7, 3, true, false, false);

            return new TextRangeCollection<TextRange>(ranges);
        }

        TextRangeCollection<TextRange> MakeInclusiveCollectionWithGaps()
        {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = new InclusiveTextRange(1, 3, true, false, false);
            ranges[1] = new InclusiveTextRange(7, 3, true, true, true);
            ranges[2] = new InclusiveTextRange(13, 3, true, false, false);

            return new TextRangeCollection<TextRange>(ranges);
        }

        private void AssertEquals(TextRangeCollection<TextRange> target, params int[] values)
        {
            Assert.AreEqual(target.Count, values.Length / 2);
            for(int i = 0; i < values.Length; i += 2)
            {
                Assert.AreEqual(values[i], target[i / 2].Start);
                Assert.AreEqual(values[i + 1], target[i / 2].End);
            }
        }

        [TestMethod]
        public void TextRangeCollection_InclusiveTestEditInMiddle()
        {
            TextRangeCollection<TextRange> target = MakeInclusiveCollection();
            AssertEquals(target, 1, 4, 4, 7, 7, 10);

            // Add text in middle of range
            target.ReflectTextChange(5, 0, 1);
            AssertEquals(target, 1, 4, 4, 8, 8, 11);

            // Remove text in middle of range
            target.ReflectTextChange(5, 1, 0);
            AssertEquals(target, 1, 4, 4, 7, 7, 10);

            // Add and Remove text in middle of range
            target.ReflectTextChange(5, 1, 1);
            AssertEquals(target, 1, 4, 4, 7, 7, 10);
        }

        [TestMethod]
        public void TextRangeCollection_InclusiveTestEditOnSeam()
        {
            TextRangeCollection<TextRange> target = MakeInclusiveCollection();
            AssertEquals(target, 1, 4, 4, 7, 7, 10);

            // Add text at beginning of !IsStartInclusive
            target.ReflectTextChange(1, 0, 1);
            AssertEquals(target, 2, 5, 5, 8, 8, 11);

            // Add text beween !IsEndInclusive, IsStartInclusive
            target.ReflectTextChange(5, 0, 1);
            AssertEquals(target, 2, 5, 5, 9, 9, 12);

            // Add text beween IsEndInclusive, !IsStartInclusive
            target.ReflectTextChange(9, 0, 1);
            AssertEquals(target, 2, 5, 5, 10, 10, 13);

            // Add text at end of !IsEndInclusive
            target.ReflectTextChange(13, 0, 1);
            AssertEquals(target, 2, 5, 5, 10, 10, 13);
        }

        [TestMethod]
        public void TextRangeCollection_InclusiveTestDeleteZeroLength()
        {
            TextRangeCollection<TextRange> target = MakeInclusiveCollection();
            AssertEquals(target, 1, 4, 4, 7, 7, 10);

            // Delete range with !allowZeroLength
            target.ReflectTextChange(1, 3, 0);
            AssertEquals(target, 1, 4, 4, 7);

            // Delete exact range with allowZeroLength
            target.ReflectTextChange(1, 3, 0);
            AssertEquals(target, 1, 1, 1, 4);

            // Delete containing range with allowZeroLength
            target.ReflectTextChange(0, 2, 0);
            AssertEquals(target, 0, 2);
        }

        [TestMethod]
        public void TextRangeCollection_DeleteBetweenRanges()
        {
            TextRangeCollection<TextRange> target = MakeInclusiveCollectionWithGaps();
            AssertEquals(target, 1, 4, 7, 10, 13, 16);

            // Delete range with !allowZeroLength
            target.ReflectTextChange(6, 5, 0);
            AssertEquals(target, 1, 4, 8, 11);
        }

        TextRangeCollection<TextRange> MakeCollection()
        {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            return new TextRangeCollection<TextRange>(ranges);
        }

        TextRangeCollection<TextRange> MakeCollection(params int [] positions)
        {
            TextRange[] ranges = new TextRange[positions.Length / 2];

            for (int i = 0; i < ranges.Length; i++)
            {
                int start = positions[2 * i];
                int end = positions[2 * i + 1];
                ranges[i] = TextRange.FromBounds(start, end);
            }

            return new TextRangeCollection<TextRange>(ranges);
        }

        [TestMethod]
        public void TextRangeCollection_ConstructorTest()
        {
            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(0, target.Start);
            Assert.AreEqual(0, target.End);
            Assert.AreEqual(0, target.Length);
        }

        [TestMethod]
        public void TextRangeCollection_ConstructorTest1()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(7, target.End);
            Assert.AreEqual(6, target.Length);

            Assert.AreEqual(1, target[0].Start);
            Assert.AreEqual(3, target[1].Start);
            Assert.AreEqual(5, target[2].Start);
        }

        [TestMethod]
        public void TextRangeCollection_AddTest()
        {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            Assert.AreEqual(0, target.Count);

            target.Add(ranges[0]);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(2, target.End);
            Assert.AreEqual(1, target.Length);
            Assert.AreEqual(1, target[0].Start);

            target.Add(ranges[1]);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(5, target.End);
            Assert.AreEqual(4, target.Length);
            Assert.AreEqual(1, target[0].Start);
            Assert.AreEqual(3, target[1].Start);

            target.Add(ranges[2]);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(7, target.End);
            Assert.AreEqual(6, target.Length);
            Assert.AreEqual(1, target[0].Start);
            Assert.AreEqual(3, target[1].Start);
            Assert.AreEqual(5, target[2].Start);
        }

        [TestMethod]
        public void TextRangeCollection_AddTest1()
        {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            target.Add(ranges);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(7, target.End);
            Assert.AreEqual(6, target.Length);

            Assert.AreEqual(1, target[0].Start);
            Assert.AreEqual(3, target[1].Start);
            Assert.AreEqual(5, target[2].Start);
        }

        [TestMethod]
        public void TextRangeCollection_ClearTest()
        {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(0, target.Start);
            Assert.AreEqual(0, target.End);
            Assert.AreEqual(0, target.Length);

            target.Clear();

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(0, target.Start);
            Assert.AreEqual(0, target.End);
            Assert.AreEqual(0, target.Length);

            target.Add(ranges);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(7, target.End);
            Assert.AreEqual(6, target.Length);

            target.Clear();

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(0, target.Start);
            Assert.AreEqual(0, target.End);
            Assert.AreEqual(0, target.Length);
        }

        [TestMethod]
        public void TextRangeCollection_ContainsTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.IsTrue(target.Contains(1));
            Assert.IsTrue(target.Contains(2));
            Assert.IsTrue(target.Contains(3));
            Assert.IsTrue(target.Contains(4));
            Assert.IsTrue(target.Contains(5));
            Assert.IsTrue(target.Contains(6));

            Assert.IsFalse(target.Contains(-10));
            Assert.IsFalse(target.Contains(0));
            Assert.IsFalse(target.Contains(7));
            Assert.IsFalse(target.Contains(Int32.MaxValue));
        }

        [TestMethod]
        public void TextRangeCollection_GetFirstItemAfterOrAtPositionTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.AreEqual(0, target.GetFirstItemAfterOrAtPosition(0));
            Assert.AreEqual(0, target.GetFirstItemAfterOrAtPosition(-2));

            Assert.AreEqual(0, target.GetFirstItemAfterOrAtPosition(1));
            Assert.AreEqual(1, target.GetFirstItemAfterOrAtPosition(2));

            Assert.AreEqual(1, target.GetFirstItemAfterOrAtPosition(3));
            Assert.AreEqual(1, target.GetFirstItemAfterOrAtPosition(4));
            Assert.AreEqual(2, target.GetFirstItemAfterOrAtPosition(5));

            Assert.AreEqual(-1, target.GetFirstItemAfterOrAtPosition(10));
            Assert.AreEqual(-1, target.GetFirstItemAfterOrAtPosition(Int32.MaxValue));
        }

        [TestMethod]
        public void TextRangeCollection_GetFirstItemBeforePositionTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            // 1-2, 3-5, 5-7


            Assert.AreEqual(-1, target.GetFirstItemBeforePosition(0));
            Assert.AreEqual(-1, target.GetFirstItemBeforePosition(-2));

            Assert.AreEqual(-1, target.GetFirstItemBeforePosition(1));

            Assert.AreEqual(0, target.GetFirstItemBeforePosition(2));
            Assert.AreEqual(0, target.GetFirstItemBeforePosition(3));
            Assert.AreEqual(0, target.GetFirstItemBeforePosition(4));

            Assert.AreEqual(1, target.GetFirstItemBeforePosition(5));
            Assert.AreEqual(1, target.GetFirstItemBeforePosition(6));

            Assert.AreEqual(2, target.GetFirstItemBeforePosition(7));
            Assert.AreEqual(2, target.GetFirstItemBeforePosition(8));
            Assert.AreEqual(2, target.GetFirstItemBeforePosition(9));
            Assert.AreEqual(2, target.GetFirstItemBeforePosition(10));
            Assert.AreEqual(2, target.GetFirstItemBeforePosition(Int32.MaxValue));
        }

        [TestMethod]
        public void TextRangeCollection_GetItemAtPositionTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.AreEqual(-1, target.GetItemAtPosition(0));
            Assert.AreEqual(-1, target.GetItemAtPosition(-2));

            Assert.AreEqual(0, target.GetItemAtPosition(1));
            Assert.AreEqual(-1, target.GetItemAtPosition(2));

            Assert.AreEqual(1, target.GetItemAtPosition(3));
            Assert.AreEqual(-1, target.GetItemAtPosition(4));
            Assert.AreEqual(2, target.GetItemAtPosition(5));

            Assert.AreEqual(-1, target.GetItemAtPosition(10));
            Assert.AreEqual(-1, target.GetItemAtPosition(Int32.MaxValue));
        }

        [TestMethod]
        public void TextRangeCollection_GetItemContainingTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            Assert.AreEqual(-1, target.GetItemContaining(0));
            Assert.AreEqual(-1, target.GetItemContaining(-2));

            Assert.AreEqual(0, target.GetItemContaining(1));
            Assert.AreEqual(-1, target.GetItemContaining(2));

            Assert.AreEqual(1, target.GetItemContaining(3));
            Assert.AreEqual(1, target.GetItemContaining(4));
            Assert.AreEqual(2, target.GetItemContaining(5));
            Assert.AreEqual(2, target.GetItemContaining(6));
            Assert.AreEqual(-1, target.GetItemContaining(7));

            Assert.AreEqual(-1, target.GetItemContaining(10));
            Assert.AreEqual(-1, target.GetItemContaining(Int32.MaxValue));
        }

        [TestMethod]
        public void TextRangeCollection_GetItemsContainingInclusiveEndTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            IReadOnlyList<int> list = target.GetItemsContainingInclusiveEnd(0);
            Assert.AreEqual(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(-2);
            Assert.AreEqual(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(1);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0]);

            list = target.GetItemsContainingInclusiveEnd(2);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0]);

            list = target.GetItemsContainingInclusiveEnd(3);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0]);

            list = target.GetItemsContainingInclusiveEnd(4);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0]);

            list = target.GetItemsContainingInclusiveEnd(5);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);

            list = target.GetItemsContainingInclusiveEnd(6);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(2, list[0]);

            list = target.GetItemsContainingInclusiveEnd(7);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(2, list[0]);

            list = target.GetItemsContainingInclusiveEnd(8);
            Assert.AreEqual(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(10);
            Assert.AreEqual(0, list.Count);

            list = target.GetItemsContainingInclusiveEnd(Int32.MaxValue);
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void TextRangeCollection_IsEqualTest()
        {
            TextRangeCollection<TextRange> target1 = MakeCollection();
            TextRangeCollection<TextRange> target2 = MakeCollection();

            Assert.IsTrue(target1.IsEqual(target2));

            target1.RemoveAt(2);
            Assert.IsFalse(target1.IsEqual(target2));

            target2.RemoveAt(2);
            Assert.IsTrue(target1.IsEqual(target2));

            target1[0].Shift(-1);
            target2[0].Shift(-1);
            Assert.IsTrue(target1.IsEqual(target2));

            target1.RemoveAt(0);
            target1.Add(TextRange.FromBounds(0, 1));
            Assert.IsFalse(target1.IsEqual(target2));

            target1.Sort();
            Assert.IsTrue(target1.IsEqual(target2));
        }

        [TestMethod]
        public void TextRangeCollection_ItemsInRangeTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            IReadOnlyList<TextRange> list = target.ItemsInRange(TextRange.EmptyRange);
            Assert.AreEqual(0, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(-10, -1));
            Assert.AreEqual(0, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(0, Int32.MaxValue));
            Assert.AreEqual(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2));
            Assert.AreEqual(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 7));
            Assert.AreEqual(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(0, 8));
            Assert.AreEqual(3, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 1));
            Assert.AreEqual(0, list.Count); // Zero-length ranges can't contain anything

            list = target.ItemsInRange(TextRange.FromBounds(1, 2));
            Assert.AreEqual(1, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 3));
            Assert.AreEqual(1, list.Count);

            list = target.ItemsInRange(TextRange.FromBounds(1, 4));
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveAtTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.RemoveAt(0);
            AssertEquals(target, 3, 5, 5, 7);

            target.RemoveAt(1);
            AssertEquals(target, 3, 5);

            target.RemoveAt(0);
            AssertEquals(target);
        }

        [TestMethod]
        public void TextRangeCollection_ShiftTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.Shift(2);
            AssertEquals(target, 3, 4, 5, 7, 7, 9);

            target.Shift(-3);
            AssertEquals(target, 0, 1, 2, 4, 4, 6);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest1()
        {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            TextRange range = TextRange.FromBounds(1, 1);

            target.RemoveInRange(range);
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.RemoveInRange(TextRange.FromBounds(1, 2));
            AssertEquals(target, 3, 5, 5, 7);

            target.RemoveInRange(TextRange.FromBounds(5, 6));
            AssertEquals(target, 3, 5);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest2()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2));
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest3()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(1, 7));
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest4()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(1, 6));
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest5()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 6));
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest6()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 5));
            AssertEquals(target, 5, 7);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest7()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 1));
            Assert.AreEqual(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(2, 3));
            Assert.AreEqual(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(5, 5));
            Assert.AreEqual(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(7, 10));
            Assert.AreEqual(3, target.Count);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveInRangeTest8()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 0), inclusiveEnds: true);
            Assert.AreEqual(3, target.Count);

            target.RemoveInRange(TextRange.FromBounds(0, 1), inclusiveEnds: true);
            Assert.AreEqual(2, target.Count);

            target.RemoveInRange(TextRange.FromBounds(5, 5), inclusiveEnds: true);
            Assert.AreEqual(1, target.Count);
        }

        [TestMethod]
        public void TextRangeCollection_ReflectTextChangeTest1()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(0, 0, 1);
            AssertEquals(target, 2, 3, 4, 6, 6, 8);

            target.ReflectTextChange(0, 1, 0);
            AssertEquals(target, 1, 2, 3, 5, 5, 7);
        }

        [TestMethod]
        public void TextRangeCollection_ReflectTextChangeTest2()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(3, 0, 3);
            AssertEquals(target, 1, 2, 3, 8, 8, 10);

            target.ReflectTextChange(8, 1, 0);
            AssertEquals(target, 1, 2, 3, 8, 8, 9);

            target.ReflectTextChange(8, 1, 0);
            AssertEquals(target, 1, 2, 3, 8);

            target.ReflectTextChange(7, 1, 0);
            AssertEquals(target, 1, 2, 3, 7);
        }

        [TestMethod]
        public void TextRangeCollection_ReflectTextChangeTest3()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(3, 1, 4);
            AssertEquals(target, 1, 2, 3, 8, 8, 10);

            target.ReflectTextChange(0, 15, 20);
            AssertEquals(target);
        }

        [TestMethod]
        public void TextRangeCollection_ReflectTextChangeTest4()
        {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.ReflectTextChange(3, 0, 3);
            AssertEquals(target, 1, 2, 3, 8, 8, 10);
        }

        [TestMethod]
        public void TextRangeCollection_ShiftStartingFromTest()
        {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ShiftStartingFrom(3, 4);
            AssertEquals(target, 1, 2, 3, 9, 9, 11);

            target.ShiftStartingFrom(1, -1);
            AssertEquals(target, 0, 1, 2, 8, 8, 10);

            target.ShiftStartingFrom(22, 10);
            AssertEquals(target, 0, 1, 2, 8, 8, 10);
        }

        [TestMethod]
        public void TextRangeCollection_ReplaceRangeTest()
        {
            TextRangeCollection<TextRange> target;
            TextRangeCollection<TextRange> toInsert;

            target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            // test insertion at beginning
            toInsert = MakeCollection(0, 1);
            target.ReplaceRange(0, 0, toInsert);
            AssertEquals(target, 0, 1, 1, 2, 3, 5, 5, 7);

            // test insertion at end
            toInsert = MakeCollection(8, 9);
            target.ReplaceRange(4, 0, toInsert);
            AssertEquals(target, 0, 1, 1, 2, 3, 5, 5, 7, 8, 9);

            // test insertion in middle
            toInsert = MakeCollection(2, 3);
            target.ReplaceRange(2, 0, toInsert);
            AssertEquals(target, 0, 1, 1, 2, 2, 3, 3, 5, 5, 7, 8, 9);

            // test deletion at beginning
            toInsert = new TextRangeCollection<TextRange>();
            target.ReplaceRange(0, 1, toInsert);
            AssertEquals(target, 1, 2, 2, 3, 3, 5, 5, 7, 8, 9);

            // test deletion at end
            toInsert = new TextRangeCollection<TextRange>();
            target.ReplaceRange(4, 1, toInsert);
            AssertEquals(target, 1, 2, 2, 3, 3, 5, 5, 7);

            // test deletion in middle
            toInsert = new TextRangeCollection<TextRange>();
            target.ReplaceRange(1, 1, toInsert);
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            // test replace at beginning
            toInsert = MakeCollection(0, 1);
            target.ReplaceRange(0, 1, toInsert);
            AssertEquals(target, 0, 1, 3, 5, 5, 7);

            // test replace at end
            toInsert = MakeCollection(8, 9);
            target.ReplaceRange(2, 1, toInsert);
            AssertEquals(target, 0, 1, 3, 5, 8, 9);

            // test replace in middle
            toInsert = MakeCollection(2, 6);
            target.ReplaceRange(1, 1, toInsert);
            AssertEquals(target, 0, 1, 2, 6, 8, 9);

            // test replace with longer collection
            toInsert = MakeCollection(2, 3, 3, 4, 4, 5, 5, 6);
            target.ReplaceRange(1, 1, toInsert);
            AssertEquals(target, 0, 1, 2, 3, 3, 4, 4, 5, 5, 6, 8, 9);

            // test replace all
            toInsert = MakeCollection(0, 9);
            target.ReplaceRange(0, 6, toInsert);
            AssertEquals(target, 0, 9);
        }

        [TestMethod]
        public void TextRangeCollection_RemoveLastItemZeroLength()
        {
            TextRangeCollection<TextRange> target;

            target = MakeCollection(2, 2);

            // testcase for deleting last range which was zero length
            target.ReflectTextChange(1, 1, 0);
            AssertEquals(target);
        }
    }
}
