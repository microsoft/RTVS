// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestDiscoverer : FactDiscoverer {
        private readonly IMessageSink _diagnosticMessageSink;

        public TestDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink) {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute) {
            var parameters = new TestParameters(testMethod, factAttribute);
            return new TestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, parameters);
        }
    }
}