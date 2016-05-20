// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.Common.Core.Test.Threading {
    public class DelayedAsyncActionTest {
        [Test]
        public async Task InvokeTwice_AfterTimeout() {
            var count = 0;
            var action = new DelayedAsyncAction(() => Task.FromResult(Interlocked.Increment(ref count)), 100);

            action.Invoke();
            await Task.Delay(200);
            action.Invoke();
            await Task.Delay(200);

            count.Should().Be(2, "DelayedAsyncAction should be called twice");
        }

        [Test]
        public async Task InvokeTwice_DuringTimeout() {
            var count = 0;
            var action = new DelayedAsyncAction(() => Task.FromResult(Interlocked.Increment(ref count)), 200);

            action.Invoke();
            await Task.Delay(100);
            action.Invoke();
            await Task.Delay(300);

            count.Should().Be(1, "DelayedAsyncAction should be called only once");
        }

        [Test]
        public async Task InvokeTwice_DuringTimeout_Concurrent() {
            var count = 0;
            var action = new DelayedAsyncAction(() => Task.FromResult(Interlocked.Increment(ref count)), 200);

            ParallelTools.Invoke(4, async i => {
                await Task.Delay(50 * i);
                action.Invoke();
            });
            await Task.Delay(500);

            count.Should().Be(1, "DelayedAsyncAction should be called only once");
        }

        [Test]
        public async Task Invoke_BackgroundThread() {
            var threadId = UIThreadHelper.Instance.Thread.ManagedThreadId;
            var action = new DelayedAsyncAction(() => {
                threadId = Thread.CurrentThread.ManagedThreadId;
                return Task.CompletedTask;
            });

            await InUI(() => action.Invoke());
            await Task.Delay(50);

            threadId.Should().NotBe(UIThreadHelper.Instance.Thread.ManagedThreadId);
        }
    }
}