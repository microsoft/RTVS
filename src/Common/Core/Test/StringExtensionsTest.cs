using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test {
    [ExcludeFromCodeCoverage]
    public class StringExtensionsTest {
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