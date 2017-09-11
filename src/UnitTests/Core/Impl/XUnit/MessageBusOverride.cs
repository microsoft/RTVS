// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UnitTests.Core.XUnit.MessageBusInjections;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class MessageBusOverride : IMessageBus {
        private readonly IMessageBus _innerMessageBus;
        private readonly List<Func<IMessageBus, TestCaseStarting, bool>> _testCaseStartingInjections = new List<Func<IMessageBus, TestCaseStarting, bool>>();
        private readonly List<Func<IMessageBus, TestStarting, bool>> _testStartingInjections = new List<Func<IMessageBus, TestStarting, bool>>();
        private readonly List<Func<IMessageBus, TestFinished, bool>> _testFinishedInjections = new List<Func<IMessageBus, TestFinished, bool>>();
        private readonly List<Func<IMessageBus, TestCaseFinished, bool>> _testCaseFinishedInjections = new List<Func<IMessageBus, TestCaseFinished, bool>>();

        public MessageBusOverride(IMessageBus innerMessageBus) {
            _innerMessageBus = innerMessageBus;
        }

        public void Dispose() {
        }

        public bool QueueMessage(IMessageSinkMessage message) {
            return RunInjections(_testCaseStartingInjections, message)
                && RunInjections(_testStartingInjections, message)
                && RunInjections(_testFinishedInjections, message)
                && RunInjections(_testCaseFinishedInjections, message)
                && _innerMessageBus.QueueMessage(message);
        }

        public MessageBusOverride AddAfterStartingBeforeFinished(ITestCaseAfterStartingBeforeFinishedInjection injection) {
            _testCaseStartingInjections.Add(injection.AfterStarting);
            _testCaseFinishedInjections.Add(injection.BeforeFinished);
            return this;
        }

        public MessageBusOverride AddAfterStartingBeforeFinished(ITestAfterStartingBeforeFinishedInjection injection) {
            _testStartingInjections.Add(injection.AfterStarting);
            _testFinishedInjections.Add(injection.BeforeFinished);
            return this;
        }

        private bool RunInjections<T>(List<Func<IMessageBus, T, bool>> injections, IMessageSinkMessage message) where T : class {
            T typedMessage = message as T;
            return typedMessage == null
                   || !injections.Any()
                   || injections.Aggregate(true, (current, injection) => current && injection(_innerMessageBus, typedMessage));
        }
    }
}