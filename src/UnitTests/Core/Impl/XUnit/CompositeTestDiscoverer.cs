// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class CompositeTestDiscoverer : TheoryDiscoverer {
        private readonly IMessageSink _diagnosticMessageSink;

        public CompositeTestDiscoverer(IMessageSink diagnosticMessageSink)
            : base (diagnosticMessageSink) {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow) {
            var parameters = new TestParameters(testMethod, theoryAttribute);
            yield return new TestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, parameters, dataRow);
        }
    }
}