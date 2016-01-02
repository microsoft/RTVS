using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Tests
{
    public class StackExtensionsTest
    {
        [Test]
        public void PopWhile()
        {
            var stack = new Stack<int>(Enumerable.Range(0, 10));
            var actual = stack.PopWhile(i => i%5 != 0);
            actual.Should().Equal(9, 8, 7, 6);
        }
    }
}