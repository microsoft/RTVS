using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public class TestMethodInfoFixture {
        public MethodInfo Method { get; }

        public TestMethodInfoFixture() { }

        public TestMethodInfoFixture(MethodInfo method) {
            Method = method;
        }
    }
}