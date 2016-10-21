// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.Common.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    public class DelayedAsyncActionTest {
        [Test]
        public async Task InvokeTwice_AfterTimeout() {
            var count = 0;
            var action = new DelayedAsyncAction(100, () => Task.FromResult(Interlocked.Increment(ref count)));

            action.Invoke();
            await Task.Delay(200);
            action.Invoke();
            await Task.Delay(200);

            count.Should().Be(2, "DelayedAction should be called twice");
        }

        [Test]
        public async Task InvokeTwice_DuringTimeout() {
            var count = 0;
            var action = new DelayedAsyncAction(200, () => Task.FromResult(Interlocked.Increment(ref count)));

            action.Invoke();
            await Task.Delay(100);
            action.Invoke();
            await Task.Delay(300);

            count.Should().Be(1, "DelayedAction should be called only once");
        }

        [Test]
        public async Task InvokeTwice_DuringTimeout_Concurrent() {
            var count = 0;
            var action = new DelayedAsyncAction(200, () => Task.FromResult(Interlocked.Increment(ref count)));

            ParallelTools.Invoke(4, async i => {
                await Task.Delay(50 * i);
                action.Invoke();
            });
            await Task.Delay(500);

            count.Should().Be(1, "DelayedAction should be called only once");
        }

        [Test]
        public async Task InvokeTwice_DuringTimeout_ChangeAction() {
            var count1 = 0;
            var count2 = 0;
            var action = new DelayedAsyncAction(250);

            action.Invoke(() => Task.FromResult(Interlocked.Increment(ref count1)));
            await Task.Delay(50);
            action.Invoke(() => Task.FromResult(Interlocked.Increment(ref count2)));
            await Task.Delay(450);

            count1.Should().Be(0, "DelayedAction should not be called for the first action");
            count2.Should().Be(1, "DelayedAction should be called only once for the second action");
        }

        [Test]
        public async Task Invoke_Concurrent_ChangeAction() {
            var counts = new int[4];
            var action = new DelayedAsyncAction(200);

            ParallelTools.Invoke(4, i => {
                action.Invoke(() => Task.FromResult(Interlocked.Increment(ref counts[i])));
            });
            await Task.Delay(400);

            counts.Should().ContainSingle(i => i == 1, "DelayedAction should be called only once")
                .And.OnlyContain(i => i <= 1, "DelayedAction should be called more than once");
        }

        [Test]
        public async Task Invoke_BackgroundThread() {
            var threadId = UIThreadHelper.Instance.Thread.ManagedThreadId;
            var action = new DelayedAsyncAction(0, () => {
                threadId = Thread.CurrentThread.ManagedThreadId;
                return Task.CompletedTask;
            });

            await InUI(() => action.Invoke());
            await Task.Delay(200);

            threadId.Should().NotBe(UIThreadHelper.Instance.Thread.ManagedThreadId);
        }
    }
}