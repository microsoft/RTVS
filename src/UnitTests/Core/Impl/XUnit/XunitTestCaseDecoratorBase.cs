// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.XUnit.MessageBusInjections;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    internal abstract class XunitTestCaseDecoratorBase : LongLivedMarshalByRefObject, IXunitTestCase {
        private IXunitTestCase _testCase;
        private bool _suppressDebugFail;
        private string _displayName;

        protected IXunitTestCase TestCase => _testCase;
        protected bool SuppressDebugFail => _suppressDebugFail;

        protected XunitTestCaseDecoratorBase(IXunitTestCase testCase) {
            _testCase = testCase;
        }

        public string DisplayName
        {
            get
            {
                if (_displayName != null) {
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

        public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) {
            TestTraceListener.Ensure();
            MessageBusOverride messageBusOverride = new MessageBusOverride(messageBus)
                .AddAfterStartingBeforeFinished(new ExecuteBeforeAfterAttributesMessageBusInjection(Method, _testCase.TestMethod.TestClass.Class));

            var testInformationFixtureIndex = Array.IndexOf(constructorArguments, ClassRunner.TestMethodFixtureDummy);
            if (testInformationFixtureIndex == -1) {
                return RunAsyncOverride(diagnosticMessageSink, messageBusOverride, constructorArguments, aggregator, cancellationTokenSource);
            }

            var testMethodFixture = new TestMethodFixture(_testCase.TestMethod.Method.ToRuntimeMethod());
            var constructorArgumentsCopy = InjectTestMethodFixture(constructorArguments, testInformationFixtureIndex, testMethodFixture);
            var testCaseRunTask = RunAsyncOverride(diagnosticMessageSink, messageBusOverride, constructorArgumentsCopy, aggregator, cancellationTokenSource);
            var observeTask = CreateObserveTask(testMethodFixture.FailedObservedTask, messageBusOverride, cancellationTokenSource);

            return Task.WhenAny(testCaseRunTask, observeTask)
                .Unwrap()
                .ContinueWith((t, s) => {
                    ((IDisposable) s).Dispose();
                    return t.Result;
                }, testMethodFixture, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        protected abstract Task<RunSummary> RunAsyncOverride(IMessageSink diagnosticMessageSink, MessageBusOverride messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource);

        public void Deserialize(IXunitSerializationInfo info) {
            _testCase = info.GetValue<IXunitTestCase>("testCase");
            _suppressDebugFail = info.GetValue<bool>("suppressDebugFail");
        }

        public void Serialize(IXunitSerializationInfo info) {
            info.AddValue("testCase", _testCase);
            info.AddValue("suppressDebugFail", _suppressDebugFail);
        }

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
                messageBus.QueueMessage(new TestFailed(new XunitTest(TestCase, DisplayName), runSummary.Time, string.Empty, t.Exception));
                cancellationTokenSource.Cancel();
                tcs.SetResult(runSummary);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }
}