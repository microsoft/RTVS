// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    public class TextRangeTest {
        [Test]
        [Category.Languages.Core]
        public void TextRange_IntersectTest01() {
            TextRange r1 = TextRange.FromBounds(1, 5);
            TextRange r2 = TextRange.FromBounds(5, 10);

            TextRange.Intersect(r1, r2).Should().BeFalse();
            TextRange.Intersect(r2, r1).Should().BeFalse();

            TextRange.Intersect(r1, r1).Should().BeTrue();

            TextRange r3 = TextRange.FromBounds(1, 1);
            TextRange r4 = TextRange.FromBounds(1, 2);
            TextRange r5 = TextRange.FromBounds(2, 3);

            TextRange.Intersect(r1, r3).Should().BeTrue();

            TextRange.Intersect(r1, r4).Should().BeTrue();
            TextRange.Intersect(r1, r5).Should().BeTrue();

            TextRange.Intersect(r3, r1).Should().BeTrue();
            TextRange.Intersect(r4, r1).Should().BeTrue();
            TextRange.Intersect(r5, r1).Should().BeTrue();

            TextRange r6 = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            TextRange.Intersect(r1, r6).Should().BeTrue();
            TextRange.Intersect(r6, r1).Should().BeTrue();

            TextRange r7 = TextRange.FromBounds(0, 20);
            TextRange.Intersect(r1, r7).Should().BeTrue();
            TextRange.Intersect(r7, r1).Should().BeTrue();

            TextRange r8 = TextRange.FromBounds(5, 8);
            TextRange.Intersect(r1, r8).Should().BeFalse();
            TextRange.Intersect(r8, r1).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_IntersectTest02() {
            TextRange r1 = TextRange.FromBounds(1, 5);
            TextRange r2 = TextRange.FromBounds(5, 10);

            r1.Intersect(r2).Should().BeFalse();
            r2.Intersect(r1).Should().BeFalse();

            r1.Intersect(r1).Should().BeTrue();

            TextRange r3 = TextRange.FromBounds(1, 1);
            TextRange r4 = TextRange.FromBounds(1, 2);
            TextRange r5 = TextRange.FromBounds(2, 3);

            r1.Intersect(r3).Should().BeTrue();

            r1.Intersect(r4).Should().BeTrue();
            r1.Intersect(r5).Should().BeTrue();

            r3.Intersect(r1).Should().BeTrue();
            r4.Intersect(r1).Should().BeTrue();
            r5.Intersect(r1).Should().BeTrue();

            TextRange r6 = TextRange.FromBounds(Int32.MinValue / 2, Int32.MaxValue / 2);
            r1.Intersect(r6).Should().BeTrue();
            r6.Intersect(r1).Should().BeTrue();

            TextRange r7 = TextRange.FromBounds(0, 20);
            r1.Intersect(r7).Should().BeTrue();
            r7.Intersect(r1).Should().BeTrue();

            TextRange r8 = TextRange.FromBounds(5, 8);
            r1.Intersect(r8).Should().BeFalse();
            r8.Intersect(r1).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_IsValidTest() {
            TextRange.IsValid(TextRange.EmptyRange).Should().BeFalse();

            TextRange r1 = TextRange.FromBounds(1, 1);
            TextRange.IsValid(r1).Should().BeFalse();

            TextRange r2 = TextRange.FromBounds(1, 2);
            TextRange.IsValid(r2).Should().BeTrue();

            TextRange r4 = TextRange.FromBounds(2, 3);
            TextRange.IsValid(r4).Should().BeTrue();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_CompareToTest() {
            TextRange r1 = TextRange.FromBounds(1, 1);

            r1.Equals(new object()).Should().BeFalse();
            r1.Equals(null).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ConstructionTest1() {
            Action a = () => TextRange.FromBounds(2, 1);
            a.ShouldThrow<Exception>();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ConstructionTest2() {
            Action a = () => TextRange.FromBounds(Int32.MinValue, Int32.MaxValue);
            a.ShouldThrow<Exception>();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ConstructionTest3() {
            Action a = () => TextRange.FromBounds(int.MinValue/2, Int32.MaxValue/2);
            a.ShouldNotThrow();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ConstructionTest4() {
            TextRange r = new TextRange(0);

            r.Start.Should().Be(0);
            r.Length.Should().Be(1);

            r = new TextRange();

            r.Start.Should().Be(0);
            r.Length.Should().Be(1);

            r = new TextRange(Int32.MaxValue);

            r.Start.Should().Be(Int32.MaxValue);
            r.Length.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ContainsTest() {
            TextRange r = TextRange.FromBounds(1, 3);

            TextRange.Contains(r, Int32.MinValue).Should().BeFalse();
            TextRange.Contains(r, 0).Should().BeFalse();

            TextRange.Contains(r, 1).Should().BeTrue();
            TextRange.Contains(r, 2).Should().BeTrue();

            TextRange.Contains(r, 3).Should().BeFalse();
            TextRange.Contains(r, Int32.MaxValue).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ContainsTest1() {
            TextRange r = TextRange.FromBounds(1, 5);

            TextRange.Contains(r, TextRange.FromBounds(Int32.MinValue / 2, 0)).Should().BeFalse();
            TextRange.Contains(r, TextRange.FromBounds(0, 1)).Should().BeFalse();

            TextRange.Contains(r, TextRange.FromBounds(5, 6)).Should().BeFalse();
            TextRange.Contains(r, TextRange.FromBounds(5, Int32.MaxValue / 2)).Should().BeFalse();

            TextRange.Contains(r, TextRange.FromBounds(1, 2)).Should().BeTrue();
            TextRange.Contains(r, TextRange.FromBounds(3, 4)).Should().BeTrue();

            TextRange.Contains(r, TextRange.FromBounds(1, 5)).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_ContainsTest2() {
            TextRange r = TextRange.FromBounds(1, 5);

            r.Contains(TextRange.FromBounds(Int32.MinValue / 2, 0)).Should().BeFalse();
            r.Contains(TextRange.FromBounds(0, 1)).Should().BeFalse();

            r.Contains(TextRange.FromBounds(5, 6)).Should().BeFalse();
            r.Contains(TextRange.FromBounds(5, Int32.MaxValue / 2)).Should().BeFalse();

            r.Contains(TextRange.FromBounds(1, 2)).Should().BeTrue();
            r.Contains(TextRange.FromBounds(3, 4)).Should().BeTrue();

            r.Contains(TextRange.FromBounds(1, 5)).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_EmptyTest() {
            TextRange r = TextRange.FromBounds(1, 2);
            r.Empty();

            TextRange.IsValid(r).Should().BeFalse();
            r.Start.Should().Be(0);
            r.End.Should().Be(0);
            r.Length.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TextRange_AreEqualTest() {
            TextRange r = TextRange.FromBounds(1, 2);
            TextRange r1 = TextRange.FromBounds(1, 2);

            TextRange.AreEqual(r, TextRange.EmptyRange).Should().BeFalse();
            TextRange.AreEqual(r, r).Should().BeTrue();
            TextRange.AreEqual(r, null).Should().BeFalse();
            TextRange.AreEqual(null, r).Should().BeFalse();
            TextRange.AreEqual(r, r1).Should().BeTrue();
        }
    }
}
