// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Testing;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class XunitTestEnvironment : ITestEnvironment {
        private readonly AsyncLocal<TaskObserver> _taskWaitingContext = new AsyncLocal<TaskObserver>();
        public void AddTaskToWait(Task task) => _taskWaitingContext.Value.Add(task);

        internal TaskObserver UseTaskObserver() {
            if (_taskWaitingContext.Value != null) {
                throw new InvalidOperationException("AsyncLocal<TaskObserver> reentrancy");
            }

            var context = new TaskObserver(RemoveTaskObserver);
            _taskWaitingContext.Value = context;
            return context;
        }

        private void RemoveTaskObserver() => _taskWaitingContext.Value = null;
    }
}