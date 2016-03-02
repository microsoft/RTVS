// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    /// <summary>
    /// Logic of this class requires change of the test method arguments, that cannot be done from decorator.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestForTypesXunitTestCase : XunitTestCase
    {
        public TestForTypesXunitTestCase() : base(null, default(TestMethodDisplay), null, null)
        {
        }

        public TestForTypesXunitTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, Type testMethodArgumentType)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, new object[] { testMethodArgumentType })
        {
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            Type testMethodArgumentType = (Type)TestMethodArguments.First();
            object testMethodArgument = Activator.CreateInstance(testMethodArgumentType);
            return new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, new []{ testMethodArgument }, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }
    }
}