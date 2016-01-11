using System;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer {
        public Type GetTestFrameworkType(IAttributeInfo attribute) {
            return typeof (TestFramework);
        }
    }
}