using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer {
        public Type GetTestFrameworkType(IAttributeInfo attribute) {
            return typeof (TestFramework);
        }
    }
}