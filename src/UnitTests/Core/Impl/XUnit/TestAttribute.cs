using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [XunitTestCaseDiscoverer("Microsoft.UnitTests.Core.XUnit.TestDiscoverer", "Microsoft.UnitTests.Core")]
    [TraitDiscoverer("Microsoft.UnitTests.Core.XUnit.UnitTestTraitDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : FactAttribute
    {
        public TestAttribute(ThreadType threadType = ThreadType.Default)
        {
            ThreadType = threadType;
        }

        public ThreadType ThreadType { get; set; }
    }
}