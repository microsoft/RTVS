using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.XUnit.MessageBusInjections;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    internal abstract class XunitTestCaseDecoratorBase : LongLivedMarshalByRefObject, IXunitTestCase
    {
        private IXunitTestCase _testCase;
        private bool _suppressDebugFail;
        private string _displayName;

        protected IXunitTestCase TestCase => _testCase;
        protected bool SuppressDebugFail => _suppressDebugFail;

        protected XunitTestCaseDecoratorBase(IXunitTestCase testCase)
        {
            _testCase = testCase;
        }

        public string DisplayName
        {
            get
            {
                if (_displayName != null)
                {
                    return _displayName;
                }

                string name = _testCase.DisplayName;
                string className = _testCase.TestMethod.TestClass.Class.Name;
                int namespaceLength = className.LastIndexOf(".", StringComparison.Ordinal);
                _displayName = namespaceLength < name.Length ? name.Substring(namespaceLength + 1) : name;

                return _displayName;
            }
        }

        public string SkipReason => _testCase.SkipReason;

        public ISourceInformation SourceInformation
        {
            get { return _testCase.SourceInformation; } 
            set { _testCase.SourceInformation = value; }
        }

        public ITestMethod TestMethod => _testCase.TestMethod;

        public object[] TestMethodArguments => _testCase.TestMethodArguments;

        public Dictionary<string, List<string>> Traits => _testCase.Traits;

        public string UniqueID => _testCase.UniqueID;

        public IMethodInfo Method => _testCase.Method;

        public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            TestTraceListener.Ensure();
            MessageBusOverride messageBusOverride = new MessageBusOverride(messageBus)
                .AddAfterStartingBeforeFinished(new ExecuteBeforeAfterAttributesMessageBusInjection(Method, _testCase.TestMethod.TestClass.Class));
            return RunAsyncOverride(diagnosticMessageSink, messageBusOverride, constructorArguments, aggregator, cancellationTokenSource);
        }

        protected abstract Task<RunSummary> RunAsyncOverride(IMessageSink diagnosticMessageSink, MessageBusOverride messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource);

        public void Deserialize(IXunitSerializationInfo info)
        {
            _testCase = info.GetValue<IXunitTestCase>("testCase");
            _suppressDebugFail = info.GetValue<bool>("suppressDebugFail");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("testCase", _testCase);
            info.AddValue("suppressDebugFail", _suppressDebugFail);
        }
    }
}