// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Microsoft.Common.Core.Testing;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TestFramework : XunitTestFramework {
        private static readonly XunitTestEnvironment _testEnvironment;

        static TestFramework() {
            _testEnvironment = new XunitTestEnvironment();
            TestEnvironment.Current = _testEnvironment;
        }

        public TestFramework(IMessageSink messageSink) : base(messageSink) {}

        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo) 
            => new TestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink, AssemblyLoaderAttribute.GetAssemblyLoaders(assemblyInfo));

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) 
            => new TestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink, _testEnvironment);
    }
}