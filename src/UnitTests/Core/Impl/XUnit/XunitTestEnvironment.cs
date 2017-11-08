// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Testing;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class XunitTestEnvironment : ITestEnvironment {
        private readonly AsyncLocal<TaskObserver> _taskObserver = new AsyncLocal<TaskObserver>();

        public bool TryAddTaskToWait(Task task) {
            var taskObserver = _taskObserver.Value;
            if (taskObserver == null) {
                return false;
            }
            taskObserver.Add(task);
            return true;
        }

        internal TaskObserver UseTaskObserver(ITestMainThreadFixture testMainThreadFixture) {
            if (_taskObserver.Value != null) {
                throw new InvalidOperationException("AsyncLocal<TaskObserver> reentrancy");
            }

            var context = new TaskObserver(testMainThreadFixture, RemoveTaskObserver);
            _taskObserver.Value = context;
            return context;
        }

        private void RemoveTaskObserver() => _taskObserver.Value = null;
    }
}