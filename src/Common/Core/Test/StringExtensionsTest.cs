// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test {
    [ExcludeFromCodeCoverage]
    public class StringExtensionsTest {
        [CompositeTest]
        [InlineData("aaa aa", "a", 0, false, new [] {0, 1, 2, 4, 5})]
        [InlineData("aaa aa", "a", 0, true, new [] {0, 1, 2, 4, 5})]
        [InlineData("aaa aa", "a", 2, false, new [] {2, 4, 5})]
        [InlineData("aaa aa", "a", 2, true, new [] { 2, 4, 5 })]
        [InlineData("aaa aa", "aa", 0, false, new [] {0, 1, 4})]
        [InlineData("aaa aa", "aa", 0, true, new [] {0, 4})]
        [InlineData("aaa aa", "aa", 1, false, new [] {1, 4})]
        [InlineData("aaa aa", "aa", 1, true, new [] {1, 4})]
        [InlineData("aaa aa", " ", 0, false, new[] { 3 })]
        [InlineData("aaa aa", " ", 0, true, new[] { 3 })]
        [InlineData("aaa aa", "b", 0, false, new int[] { })]
        [InlineData("aaa aa", "b", 0, true, new int[] { })]
        public void AllIndexesOfIgnoreCase(string s, string value, int startIndex, bool allowOverlap, int[] expected) {
            s.AllIndexesOfIgnoreCase(value, startIndex, allowOverlap).Should().Equal(expected);
        }

        [CompositeTest]
        [InlineData(" ", 0, false, true)]
        [InlineData(" ", 0, true, true)]
        [InlineData(" ", 1, false, false)]
        [InlineData(" ", 1, true, false)]
        [InlineData("  ", 1, false, false)]
        [InlineData("  ", 1, true, true)]
        [InlineData(" \n", 1, false, false)]
        [InlineData(" \n", 1, true, false)]
        [InlineData(" \r\n", 1, false, false)]
        [InlineData(" \r\n", 1, true, false)]
        [InlineData("  \n  ", 3, false, true)]
        [InlineData("  \n  ", 3, true, true)]
        [InlineData("  \n  ", 4, false, false)]
        [InlineData("  \n  ", 4, true, true)]
        [InlineData("  \r\n  ", 4, false, true)]
        [InlineData("  \r\n  ", 4, true, true)]
        [InlineData("  \r\n  ", 5, false, false)]
        [InlineData("  \r\n  ", 5, true, true)]
        [InlineData("aa", -1, false, false)]
        [InlineData("aa", -1, true, false)]
        [InlineData("aa", 0, false, true)]
        [InlineData("aa", 0, true, true)]
        [InlineData("aa", 1, false, false)]
        [InlineData("aa", 1, true, false)]
        [InlineData("aa", 2, false, false)]
        [InlineData("aa", 2, true, false)]
        [InlineData("aa\n", 2, false, false)]
        [InlineData("aa\n", 2, true, false)]
        [InlineData("aa\r", 2, false, false)]
        [InlineData("aa\r", 2, true, false)]
        [InlineData("aa\r\n", 2, false, false)]
        [InlineData("aa\r\n", 2, true, false)]
        [InlineData("aa\r\n", 3, false, false)]
        [InlineData("aa\r\n", 3, true, false)]
        [InlineData("\raa", 0, false, false)]
        [InlineData("\raa", 0, true, false)]
        [InlineData("\raa", 1, false, true)]
        [InlineData("\raa", 1, true, true)]
        [InlineData("\raa", 2, false, false)]
        [InlineData("\raa", 2, true, false)]
        [InlineData("\naa", 0, false, false)]
        [InlineData("\naa", 0, true, false)]
        [InlineData("\naa", 1, false, true)]
        [InlineData("\naa", 1, true, true)]
        [InlineData("\naa", 2, false, false)]
        [InlineData("\naa", 2, true, false)]
        [InlineData("\r\naa", 0, false, false)]
        [InlineData("\r\naa", 0, true, false)]
        [InlineData("\r\naa", 1, false, false)]
        [InlineData("\r\naa", 1, true, false)]
        [InlineData("\r\naa", 2, false, true)]
        [InlineData("\r\naa", 2, true, true)]
        [InlineData("\r\naa", 3, false, false)]
        [InlineData("\r\naa", 3, true, false)]
        public void IsStartOfNewLine(string s, int index, bool ignoreWhitespaces, bool expected) {
            s.IsStartOfNewLine(index, ignoreWhitespaces).Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("aaaa", "a", "b", 0, 2, "bbaa")]
        [InlineData("aaaa", "a", "b", 1, 2, "abba")]
        [InlineData("aaaa", "aa", "b", 0, 2, "baa")]
        [InlineData("aaaa", "aa", "b", 0, 3, "baa")]
        [InlineData("aaaa", "aa", "b", 1, 2, "aba")]
        [InlineData("aaccaa", "aa", "b", 0, 5, "bccaa")]
        [InlineData("aaccaa", "aa", "b", 0, 6, "bccb")]
        public void Replace(string s, string oldValue, string newValue, int start, int length, string expected) {
            s.Replace(oldValue, newValue, start, length).Should().Be(expected);
        }

        [CompositeTest]
        [InlineData(" ", "")]
        [InlineData(" \n", "")]
        [InlineData(" \r\n", "")]
        [InlineData(" \r\r\n", "")]
        [InlineData("  \n  ", "")]
        [InlineData("  \r\n  ", "")]
        [InlineData("  \r\r\n  ", "")]
        [InlineData("\n", "")]
        [InlineData("\r\n", "")]
        [InlineData("\r\r\n", "")]
        [InlineData("aa", "aa")]
        [InlineData("aa\n", "aa")]
        [InlineData("aa\r", "aa")]
        [InlineData("aa\r\n", "aa")]
        [InlineData("\raa", "aa")]
        [InlineData("\naa", "aa")]
        [InlineData("\r\naa", "aa")]
        [InlineData("  aa\r\n", "  aa")]
        [InlineData("\r\n  aa\r\n", "  aa")]
        [InlineData("aa\nbb", "aa\nbb")]
        [InlineData("aa\rbb", "aa\rbb")]
        [InlineData("aa\r\nbb", "aa\r\nbb")]
        [InlineData("aa\n\r\nbb", "aa\r\nbb")]
        [InlineData("aa\r\n\r\nbb", "aa\r\nbb")]
        [InlineData("aa\r\nbb\n", "aa\r\nbb")]
        [InlineData("aa\n\r\nbb\r\n", "aa\r\nbb")]
        [InlineData("aa\r\n\r\nbb\r", "aa\r\nbb")]
        [InlineData("aa\r\n  \r\nbb\r\n", "aa\r\nbb")]
        [InlineData("aa\r\n  \r\nbb\r\n  \r\n  cc \r\n", "aa\r\nbb\r\n  cc ")]
        public void RemoveWhiteSpaceLines(string s, string expected) {
            s.RemoveWhiteSpaceLines().Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("a", 0, 1, 10)]
        [InlineData("a0Bxx", 1, 2, 11)]
        [InlineData("+1+", 1, 1, 1)]
        [InlineData("0011c", 1, 3, 17)]
        public void SubstringToHex(string s, int start, int length, int expected) {
            s.SubstringToHex(start, length).Should().Be(expected);
        }

    }
}