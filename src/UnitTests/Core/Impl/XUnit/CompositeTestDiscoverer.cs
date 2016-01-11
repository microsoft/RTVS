using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    public class CompositeTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly TheoryDiscoverer _theoryDiscoverer;

        public CompositeTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _theoryDiscoverer = new TheoryDiscoverer(diagnosticMessageSink);
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            TestParameters parameters = new TestParameters(factAttribute);
            List<IXunitTestCase> cases = _theoryDiscoverer.Discover(discoveryOptions, testMethod, factAttribute).AsList();

            if (parameters.ThreadType == ThreadType.UI)
            {
                return cases.Select(c => new XunitMainThreadTestCaseDecorator(c)).ToList();
            }

            return cases.Select(c => new XunitTestCaseDecorator(c)).ToList();
        }
    }
}