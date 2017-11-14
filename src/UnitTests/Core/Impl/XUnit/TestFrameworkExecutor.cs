// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TestFrameworkExecutor : XunitTestFrameworkExecutor {
        private readonly XunitTestEnvironment _testEnvironment;

        public TestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink, XunitTestEnvironment testEnvironment)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink) {
            _testEnvironment = testEnvironment;
        }

        protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions) {
            using (var assemblyRunner = new AssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions, _testEnvironment)) {
                await assemblyRunner.RunAsync();
            }
        }
    }
}