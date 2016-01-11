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
    }
}