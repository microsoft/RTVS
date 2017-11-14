// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    [Category.Threads]
    public class ControlledTaskSchedulerTest {
        private readonly ControlledTaskScheduler _scheduler;

        public ControlledTaskSchedulerTest() {
            _scheduler = new ControlledTaskScheduler(SynchronizationContext.Current);
        }

        [Test]
        public void ControlledTaskScheduler_ThrowException() {
            Func<Task> f = async () => await Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, _scheduler);
            f.ShouldThrow<CustomException>();
        }

        [Test]
        public void ControlledTaskScheduler_GetAwaiter_ThrowException() {
            Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, _scheduler);
            Func<Task> f = async () => await _scheduler;
            f.ShouldThrow<CustomException>();
        }

        [Test]
        public void ControlledTaskScheduler_Wait_ThrowException() {
            Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, _scheduler);
            Action a = () => _scheduler.Wait();
            a.ShouldThrow<CustomException>();
        }

        [Test]
        public void ControlledTaskScheduler_WaitForUpcomingTasks_ThrowException() {
            Task.Delay(100).ContinueWith(t => Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, _scheduler));
            Action a = () => _scheduler.WaitForUpcomingTasks(400);
            a.ShouldThrow<CustomException>();
        }

        [Test]
        public void ControlledTaskScheduler_WaitForUpcomingTasks_ThrowTimeoutException() {

            Task.Delay(200).ContinueWith(t => Task.Factory.StartNew(SleepAndThrow, CancellationToken.None, TaskCreationOptions.None, _scheduler));
            Action a = () => _scheduler.WaitForUpcomingTasks(100);
            a.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void ControlledTaskScheduler_WaitActionBlock_ThrowException() {
            Func<object, Task> f = o => Task.Factory.StartNew(SleepAndThrow);
            ActionBlock<object> actionBlock = new ActionBlock<object>(f, new ExecutionDataflowBlockOptions { TaskScheduler = _scheduler });

            Action a = () => _scheduler.Wait(actionBlock);
            actionBlock.SendAsync(null);
            a.ShouldThrow<CustomException>();
        }

        private static void SleepAndThrow() {
            Thread.Sleep(10);
            throw new CustomException();
        }

        private class CustomException : Exception {
        }
    }
}
