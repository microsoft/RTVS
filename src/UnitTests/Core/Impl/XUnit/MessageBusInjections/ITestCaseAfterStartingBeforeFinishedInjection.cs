// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MessageBusInjections {
    internal interface ITestCaseAfterStartingBeforeFinishedInjection {
        bool AfterStarting(IMessageBus messageBus, TestCaseStarting message);
        bool BeforeFinished(IMessageBus messageBus, TestCaseFinished message);
    }
}