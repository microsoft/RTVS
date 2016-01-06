using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TestFramework : XunitTestFramework {
        public TestFramework(IMessageSink messageSink) : base(messageSink) {}

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) {
            return new TestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }
}