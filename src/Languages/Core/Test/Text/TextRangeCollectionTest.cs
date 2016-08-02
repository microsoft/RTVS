// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    public class TextRangeCollectionTest {
        private static void AssertEquals(TextRangeCollection<TextRange> target, params int[] values) {
            target.Count.Should().Be(values.Length/2);

            for (int i = 0; i < values.Length; i += 2) {
                target[i / 2].Start.Should().Be(values[i]);
                target[i / 2].End.Should().Be(values[i + 1]);
            }
        }

        private static TextRangeCollection<TextRange> MakeCollection() {
            var ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            return new TextRangeCollection<TextRange>(ranges);
        }

        private static TextRangeCollection<TextRange> MakeCollection(params int[] positions) {
            var ranges = new TextRange[positions.Length / 2];

            for (int i = 0; i < ranges.Length; i++) {
                int start = positions[2 * i];
                int end = positions[2 * i + 1];
                ranges[i] = TextRange.FromBounds(start, end);
            }

            return new TextRangeCollection<TextRange>(ranges);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ConstructorTest() {
            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            target.Count.Should().Be(0);
            target.Start.Should().Be(0);
            target.End.Should().Be(0);
            target.Length.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ConstructorTest1() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.Count.Should().Be(3);
            target.Start.Should().Be(1);
            target.End.Should().Be(7);
            target.Length.Should().Be(6);

            target[0].Start.Should().Be(1);
            target[1].Start.Should().Be(3);
            target[2].Start.Should().Be(5);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_AddTest() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            target.Count.Should().Be(0);

            target.Add(ranges[0]);
            target.Count.Should().Be(1);
            target.Start.Should().Be(1);
            target.End.Should().Be(2);
            target.Length.Should().Be(1);
            target[0].Start.Should().Be(1);

            target.Add(ranges[1]);
            target.Count.Should().Be(2);
            target.Start.Should().Be(1);
            target.End.Should().Be(5);
            target.Length.Should().Be(4);
            target[0].Start.Should().Be(1);
            target[1].Start.Should().Be(3);

            target.Add(ranges[2]);
            target.Count.Should().Be(3);
            target.Start.Should().Be(1);
            target.End.Should().Be(7);
            target.Length.Should().Be(6);
            target[0].Start.Should().Be(1);
            target[1].Start.Should().Be(3);
            target[2].Start.Should().Be(5);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_AddTest1() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            target.Add(ranges);

            target.Count.Should().Be(3);
            target.Start.Should().Be(1);
            target.End.Should().Be(7);
            target.Length.Should().Be(6);

            target[0].Start.Should().Be(1);
            target[1].Start.Should().Be(3);
            target[2].Start.Should().Be(5);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ClearTest() {
            TextRange[] ranges = new TextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<TextRange> target = new TextRangeCollection<TextRange>();

            target.Count.Should().Be(0);
            target.Start.Should().Be(0);
            target.End.Should().Be(0);
            target.Length.Should().Be(0);

            target.Clear();

            target.Count.Should().Be(0);
            target.Start.Should().Be(0);
            target.End.Should().Be(0);
            target.Length.Should().Be(0);

            target.Add(ranges);

            target.Count.Should().Be(3);
            target.Start.Should().Be(1);
            target.End.Should().Be(7);
            target.Length.Should().Be(6);

            target.Clear();

            target.Count.Should().Be(0);
            target.Start.Should().Be(0);
            target.End.Should().Be(0);
            target.Length.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ContainsTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.Contains(1).Should().BeTrue();
            target.Contains(2).Should().BeTrue();
            target.Contains(3).Should().BeTrue();
            target.Contains(4).Should().BeTrue();
            target.Contains(5).Should().BeTrue();
            target.Contains(6).Should().BeTrue();

            target.Contains(-10).Should().BeFalse();
            target.Contains(0).Should().BeFalse();
            target.Contains(7).Should().BeFalse();
            target.Contains(Int32.MaxValue).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_GetFirstItemAfterOrAtPositionTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.GetFirstItemAfterOrAtPosition(0).Should().Be(0);
            target.GetFirstItemAfterOrAtPosition(-2).Should().Be(0);

            target.GetFirstItemAfterOrAtPosition(1).Should().Be(0);
            target.GetFirstItemAfterOrAtPosition(2).Should().Be(1);

            target.GetFirstItemAfterOrAtPosition(3).Should().Be(1);
            target.GetFirstItemAfterOrAtPosition(4).Should().Be(1);
            target.GetFirstItemAfterOrAtPosition(5).Should().Be(2);

            target.GetFirstItemAfterOrAtPosition(10).Should().Be(-1);
            target.GetFirstItemAfterOrAtPosition(Int32.MaxValue).Should().Be(-1);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_GetFirstItemBeforePositionTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            // 1-2, 3-5, 5-7


            target.GetFirstItemBeforePosition(0).Should().Be(-1);
            target.GetFirstItemBeforePosition(-2).Should().Be(-1);

            target.GetFirstItemBeforePosition(1).Should().Be(-1);

            target.GetFirstItemBeforePosition(2).Should().Be(0);
            target.GetFirstItemBeforePosition(3).Should().Be(0);
            target.GetFirstItemBeforePosition(4).Should().Be(0);

            target.GetFirstItemBeforePosition(5).Should().Be(1);
            target.GetFirstItemBeforePosition(6).Should().Be(1);

            target.GetFirstItemBeforePosition(7).Should().Be(2);
            target.GetFirstItemBeforePosition(8).Should().Be(2);
            target.GetFirstItemBeforePosition(9).Should().Be(2);
            target.GetFirstItemBeforePosition(10).Should().Be(2);
            target.GetFirstItemBeforePosition(Int32.MaxValue).Should().Be(2);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_GetItemAtPositionTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.GetItemAtPosition(0).Should().Be(-1);
            target.GetItemAtPosition(-2).Should().Be(-1);

            target.GetItemAtPosition(1).Should().Be(0);
            target.GetItemAtPosition(2).Should().Be(-1);

            target.GetItemAtPosition(3).Should().Be(1);
            target.GetItemAtPosition(4).Should().Be(-1);
            target.GetItemAtPosition(5).Should().Be(2);

            target.GetItemAtPosition(10).Should().Be(-1);
            target.GetItemAtPosition(Int32.MaxValue).Should().Be(-1);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_GetItemContainingTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.GetItemContaining(0).Should().Be(-1);
            target.GetItemContaining(-2).Should().Be(-1);

            target.GetItemContaining(1).Should().Be(0);
            target.GetItemContaining(2).Should().Be(-1);

            target.GetItemContaining(3).Should().Be(1);
            target.GetItemContaining(4).Should().Be(1);
            target.GetItemContaining(5).Should().Be(2);
            target.GetItemContaining(6).Should().Be(2);
            target.GetItemContaining(7).Should().Be(-1);

            target.GetItemContaining(10).Should().Be(-1);
            target.GetItemContaining(Int32.MaxValue).Should().Be(-1);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_GetItemsContainingInclusiveEndTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            IReadOnlyList<int> list = target.GetItemsContainingInclusiveEnd(0);
            list.Should().BeEmpty();

            list = target.GetItemsContainingInclusiveEnd(-2);
            list.Should().BeEmpty();

            list = target.GetItemsContainingInclusiveEnd(1);
            list.Should().HaveCount(1);
            list[0].Should().Be(0);

            list = target.GetItemsContainingInclusiveEnd(2);
            list.Should().HaveCount(1);
            list[0].Should().Be(0);

            list = target.GetItemsContainingInclusiveEnd(3);
            list.Should().HaveCount(1);
            list[0].Should().Be(1);

            list = target.GetItemsContainingInclusiveEnd(4);
            list.Should().HaveCount(1);
            list[0].Should().Be(1);

            list = target.GetItemsContainingInclusiveEnd(5);
            list.Should().HaveCount(2);
            list[0].Should().Be(1);
            list[1].Should().Be(2);

            list = target.GetItemsContainingInclusiveEnd(6);
            list.Should().HaveCount(1);
            list[0].Should().Be(2);

            list = target.GetItemsContainingInclusiveEnd(7);
            list.Should().HaveCount(1);
            list[0].Should().Be(2);

            list = target.GetItemsContainingInclusiveEnd(8);
            list.Should().BeEmpty();

            list = target.GetItemsContainingInclusiveEnd(10);
            list.Should().BeEmpty();

            list = target.GetItemsContainingInclusiveEnd(Int32.MaxValue);
            list.Should().BeEmpty();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ItemsInRangeTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            IReadOnlyList<TextRange> list = target.ItemsInRange(TextRange.EmptyRange);
            list.Should().BeEmpty();

            list = target.ItemsInRange(TextRange.FromBounds(-10, -1));
            list.Should().BeEmpty();

            list = target.ItemsInRange(TextRange.FromBounds(0, Int32.MaxValue));
            list.Should().HaveCount(3);

            list = target.ItemsInRange(TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2));
            list.Should().HaveCount(3);

            list = target.ItemsInRange(TextRange.FromBounds(1, 7));
            list.Should().HaveCount(3);

            list = target.ItemsInRange(TextRange.FromBounds(0, 8));
            list.Should().HaveCount(3);

            list = target.ItemsInRange(TextRange.FromBounds(1, 1));
            list.Should().BeEmpty(); // Zero-length ranges can't contain anything

            list = target.ItemsInRange(TextRange.FromBounds(1, 2));
            list.Should().HaveCount(1);

            list = target.ItemsInRange(TextRange.FromBounds(1, 3));
            list.Should().HaveCount(1);

            list = target.ItemsInRange(TextRange.FromBounds(1, 4));
            list.Should().HaveCount(2);
        }

        [Test]
        [Category.Languages.Core]
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

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ShiftTest() {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.Shift(2);
            AssertEquals(target, 3, 4, 5, 7, 7, 9);

            target.Shift(-3);
            AssertEquals(target, 0, 1, 2, 4, 4, 6);
        }

        [Test]
        [Category.Languages.Core]
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

            target.Start.Should().Be(3);
            target.End.Should().Be(5);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest2() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2));
            target.Count.Should().Be(0);

            target.Start.Should().Be(0);
            target.End.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest3() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(1, 7));

            target.Count.Should().Be(0);
            target.Start.Should().Be(0);
            target.End.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest4() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(1, 6));
            target.Count.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest5() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 6));
            target.Count.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest6() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 5));
            AssertEquals(target, 5, 7);

            target.Start.Should().Be(5);
            target.End.Should().Be(7);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest7() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 1));
            target.Count.Should().Be(3);

            target.RemoveInRange(TextRange.FromBounds(2, 3));
            target.Count.Should().Be(3);

            target.RemoveInRange(TextRange.FromBounds(5, 5));
            target.Count.Should().Be(3);

            target.RemoveInRange(TextRange.FromBounds(7, 10));
            target.Count.Should().Be(3);

            target.Start.Should().Be(1);
            target.End.Should().Be(7);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveInRangeTest8() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.RemoveInRange(TextRange.FromBounds(0, 0), inclusiveEnds: true);
            target.Count.Should().Be(3);

            target.RemoveInRange(TextRange.FromBounds(0, 1), inclusiveEnds: true);
            target.Count.Should().Be(2);

            target.RemoveInRange(TextRange.FromBounds(5, 5), inclusiveEnds: true);
            target.Count.Should().Be(1);

            target.Start.Should().Be(3);
            target.End.Should().Be(5);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ReflectTextChangeTest1() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(0, 0, 1);
            AssertEquals(target, 2, 3, 4, 6, 6, 8);

            target.ReflectTextChange(0, 1, 0);
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.Start.Should().Be(1);
            target.End.Should().Be(7);
        }

        [Test]
        [Category.Languages.Core]
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

            target.Start.Should().Be(1);
            target.End.Should().Be(7);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ReflectTextChangeTest3() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ReflectTextChange(3, 1, 4);
            AssertEquals(target, 1, 2, 3, 8, 8, 10);

            target.ReflectTextChange(0, 15, 20);
            AssertEquals(target);

            target.Count.Should().Be(0);
            target.Start.Should().Be(0);
            target.End.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ReflectTextChangeTest4() {
            TextRangeCollection<TextRange> target = MakeCollection();
            AssertEquals(target, 1, 2, 3, 5, 5, 7);

            target.ReflectTextChange(3, 0, 3);
            AssertEquals(target, 1, 2, 6, 8, 8, 10);

            target.Start.Should().Be(1);
            target.End.Should().Be(10);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_ShiftStartingFromTest() {
            TextRangeCollection<TextRange> target = MakeCollection();

            target.ShiftStartingFrom(3, 4);
            AssertEquals(target, 1, 2, 3, 9, 9, 11);

            target.ShiftStartingFrom(1, -1);
            AssertEquals(target, 0, 1, 2, 8, 8, 10);

            target.ShiftStartingFrom(22, 10);
            AssertEquals(target, 0, 1, 2, 8, 8, 10);

            target.Start.Should().Be(0);
            target.End.Should().Be(10);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_RemoveLastItemZeroLength() {
            TextRangeCollection<TextRange> target;

            target = MakeCollection(1, 1);

            // testcase for deleting last range which was zero length
            target.ReflectTextChange(1, 1, 0);
            target.Count.Should().Be(0);

            target.Start.Should().Be(0);
            target.End.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRangeCollection_AddSorted() {
            ITextRange[] ranges = new ITextRange[3];

            ranges[0] = TextRange.FromBounds(1, 2);
            ranges[1] = TextRange.FromBounds(3, 5);
            ranges[2] = TextRange.FromBounds(5, 7);

            TextRangeCollection<ITextRange> target = new TextRangeCollection<ITextRange>();

            target.Count.Should().Be(0);

            target.Add(ranges[2]);
            target.Count.Should().Be(1);

            target.AddSorted(ranges[0]);
            target.Count.Should().Be(2);
            target.Start.Should().Be(1);
            target.End.Should().Be(7);
            target[0].Start.Should().Be(1);
            target[1].Start.Should().Be(5);

            target.AddSorted(ranges[1]);
            target.Count.Should().Be(3);
            target.Start.Should().Be(1);
            target.End.Should().Be(7);
            target[0].Start.Should().Be(1);
            target[1].Start.Should().Be(3);
            target[2].Start.Should().Be(5);

            target.Start.Should().Be(1);
            target.End.Should().Be(7);
        }
    }
}
