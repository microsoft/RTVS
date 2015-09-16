using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    public class TestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly FactDiscoverer _factDiscoverer;

        public TestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _factDiscoverer = new FactDiscoverer(diagnosticMessageSink);
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            TestParameters parameters = new TestParameters(factAttribute);
            IXunitTestCase testCase = _factDiscoverer.Discover(discoveryOptions, testMethod, factAttribute).Single();

            if (parameters.ThreadType == ThreadType.UI)
            {
                yield return new XunitMainThreadTestCaseDecorator(testCase);
            }
            else
            {
                yield return new XunitTestCaseDecorator(testCase);
            }
        }
    }
}