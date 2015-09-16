using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    public class TestForTypesDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public TestForTypesDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            TestForTypesParameters parameters = new TestForTypesParameters(factAttribute);
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();

            if (testMethod.Method.GetParameters().Count() != 1)
            {
                return new[]
                {
                    new ExecutionErrorTestCase(_diagnosticMessageSink, methodDisplay, testMethod, "[TestForTypes] can have only one System.Type parameter")
                };
            }

            IEnumerable<TestForTypesXunitTestCase> cases = parameters.Types
                .Select(t => new TestForTypesXunitTestCase(_diagnosticMessageSink, methodDisplay, testMethod, t));

            if (parameters.ThreadType == ThreadType.UI)
            {
                return cases.Select(c => new XunitMainThreadTestCaseDecorator(c)).ToList();
            }

            return cases.Select(c => new XunitTestCaseDecorator(c)).ToList();
        }
    }
}