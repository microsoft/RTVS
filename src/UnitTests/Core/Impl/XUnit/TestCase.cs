// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit.MessageBusInjections;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestCase : XunitTestCase {
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

            XunitTestCaseRunner runner;
            var testInformationFixtureIndex = Array.IndexOf(constructorArguments, ClassRunner.TestMethodFixtureDummy);
            if (testInformationFixtureIndex == -1) {
                runner = CreateTestCaseRunner(diagnosticMessageSink, messageBusOverride, constructorArguments, aggregator, cancellationTokenSource);
                return RunAsync(runner, messageBusOverride);
            }

            var testMethodFixture = new TestMethodFixture(TestMethod.Method.ToRuntimeMethod());
            var constructorArgumentsCopy = InjectTestMethodFixture(constructorArguments, testInformationFixtureIndex, testMethodFixture);
            var observeTask = CreateObserveTask(testMethodFixture.FailedObservedTask, messageBusOverride, cancellationTokenSource);

            runner = CreateTestCaseRunner(diagnosticMessageSink, messageBusOverride, constructorArgumentsCopy, aggregator, cancellationTokenSource);
            return Task.WhenAny(RunAsync(runner, messageBusOverride), observeTask)
                .Unwrap()
                .ContinueWith((t, s) => {
                    ((IDisposable)s).Dispose();
                    return t.Result;
                }, testMethodFixture, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private Task<RunSummary> RunAsync(XunitTestCaseRunner runner, MessageBusOverride messageBus) {
            if (ThreadType == ThreadType.UI) {
                return UIThreadHelper.Instance.Invoke(runner.RunAsync);
            }

            messageBus.AddAfterStartingBeforeFinished(new VerifyGlobalProviderMessageBusInjection());
            return runner.RunAsync();
        }

        protected virtual XunitTestCaseRunner CreateTestCaseRunner(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            => new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource);

        private static object[] InjectTestMethodFixture(object[] constructorArguments, int testInformationFixtureIndex, TestMethodFixture testMethodFixture) {
            var constructorArgumentsCopy = new object[constructorArguments.Length];
            Array.Copy(constructorArguments, constructorArgumentsCopy, constructorArguments.Length);
            constructorArgumentsCopy[testInformationFixtureIndex] = testMethodFixture;
            return constructorArgumentsCopy;
        }

        private Task<RunSummary> CreateObserveTask(Task failedObservedTask, IMessageBus messageBus, CancellationTokenSource cancellationTokenSource) {
            var tcs = new TaskCompletionSource<RunSummary>();
            var stopwatch = Stopwatch.StartNew();

            failedObservedTask.ContinueWith(t => {
                stopwatch.Stop();
                if (!t.IsFaulted) {
                    return;
                }

                var runSummary = new RunSummary { Total = 1, Failed = 1, Time = (decimal)stopwatch.Elapsed.TotalSeconds };
                messageBus.QueueMessage(new TestFailed(new XunitTest(this, DisplayName), runSummary.Time, string.Empty, t.Exception));
                cancellationTokenSource.Cancel();
                tcs.SetResult(runSummary);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }
}