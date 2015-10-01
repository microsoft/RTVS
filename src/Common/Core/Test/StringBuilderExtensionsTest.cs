using System.Text;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test
{
    public class StringBuilderExtensionsTest
    {
        [Test]
        public void AppendIf_True()
        {
            var sb = new StringBuilder();
            sb.AppendIf(true, "ab");
            sb.ToString().Should().Be("ab");
        }

        [Test]
        public void AppendIf_False()
        {
            var sb = new StringBuilder();
            sb.AppendIf(false, "ab");
            sb.ToString().Should().BeEmpty();
        }
    }
}