using System;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [AttributeUsage(AttributeTargets.Assembly)]
    [TestFrameworkDiscoverer("Microsoft.UnitTests.Core.XUnit.TestFrameworkTypeDiscoverer", "Microsoft.UnitTests.Core")]
    public sealed class TestFrameworkOverrideAttribute : Attribute, ITestFrameworkAttribute {
    }
}