using System.Text;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Tests
{
    public class StringBuilderExtensionsTest
    {
        [Fact]
        public void AppendIf_True()
        {
            var sb = new StringBuilder();
            sb.AppendIf(true, "ab");
            sb.ToString().Should().Be("ab");
        }

        [Fact]
        public void AppendIf_False()
        {
            var sb = new StringBuilder();
            sb.AppendIf(false, "ab");
            sb.ToString().Should().BeEmpty();
        }
    }
}