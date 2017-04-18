// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit.MessageBusInjections;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TestCase : XunitTestCase {
        public ThreadType ThreadType { get; private set; }

        public TestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, TestParameters parameters, object[] testMethodArguments = null) 
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments) {
            ThreadType = parameters.ThreadType;
        }

        /// <summary />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TestCase() : base(null, default(TestMethodDisplay), null, null) { }

        protected override void Initialize() {
            base.Initialize();

            var className = TestMethod.TestClass.Class.Name;
            var name = DisplayName;
            var namespaceLength = className.LastIndexOf(".", StringComparison.Ordinal);
            DisplayName = namespaceLength < name.Length ? name.Substring(namespaceLength + 1) : name;
        }

        public override void Serialize(IXunitSerializationInfo data) {
            base.Serialize(data);
            data.AddValue(nameof(ThreadType), ThreadType.ToString());
        }

        public override void Deserialize(IXunitSerializationInfo data) {
            ThreadType = (ThreadType)Enum.Parse(typeof(ThreadType), data.GetValue<string>(nameof(ThreadType)));
            base.Deserialize(data);
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) {
            TestTraceListener.Ensure();
            MessageBusOverride messageBusOverride = new MessageBusOverride(messageBus)
                .AddAfterStartingBeforeFinished(new ExecuteBeforeAfterAttributesMessageBusInjection(Method, TestMethod.TestClass.Class));

            var testMethodArguments = GetTestMethodArguments();
            var runner = new TestCaseRunner(this, DisplayName, SkipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource);

            if (ThreadType == ThreadType.UI) {
                return UIThreadHelper.Instance.Invoke(runner.RunAsync);
            }

            messageBusOverride.AddAfterStartingBeforeFinished(new VerifyGlobalProviderMessageBusInjection());
            return ThreadType == ThreadType.Background ? Task.Run(() => runner.RunAsync()) : runner.RunAsync();
        }

        protected virtual object[] GetTestMethodArguments() => TestMethodArguments;
    }
}