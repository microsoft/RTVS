using System.Reflection;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestMethodInfoFixture {
        public MethodInfo Method { get; }

        public TestMethodInfoFixture() { }

        public TestMethodInfoFixture(MethodInfo method) {
            Method = method;
        }
    }
}