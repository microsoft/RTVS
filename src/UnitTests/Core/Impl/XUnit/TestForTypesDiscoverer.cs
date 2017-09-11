// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestForTypesDiscoverer : IXunitTestCaseDiscoverer {
        private readonly IMessageSink _diagnosticMessageSink;

        public TestForTypesDiscoverer(IMessageSink diagnosticMessageSink) {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            TestForTypesParameters parameters = new TestForTypesParameters(testMethod, factAttribute);
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();

            if (testMethod.Method.GetParameters().Count() != 1) {
                return new[] {
                    new ExecutionErrorTestCase(_diagnosticMessageSink, methodDisplay, testMethod, "[TestForTypes] can have only one System.Type parameter")
                };
            }

            return parameters
                .Types
                .Select(t => new TestForTypesTestCase(_diagnosticMessageSink, methodDisplay, testMethod, parameters, t))
                .ToList();
        }
    }
}