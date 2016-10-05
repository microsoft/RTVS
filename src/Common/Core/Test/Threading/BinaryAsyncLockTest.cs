// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    public class BinaryAsyncLockTest {
        [Test]
        public async Task WaitAsync_Set() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var token = await bal.WaitAsync();
                if (!token.IsSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                    token.Set();
                }
            });

            count.Should().Be(1);
        }

        [Test]
        public async Task WaitAsync_SetInCtor() {
            var bal = new BinaryAsyncLock(true);
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                await bal.WaitAsync();
                if (!bal.IsSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(0);
        }
        
        [Test]
        public async Task WaitAsync_Reset() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var token = await bal.WaitAsync();
                if (!bal.IsSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                    token.Reset();
                }
            });

            count.Should().Be(4);
        }

        [Test]
        public void ResetIfNotWaiting_ResetAsync_Skip_WaitAsync_Set() {
            var bal = new BinaryAsyncLock();
            var task = bal.ResetAsync();

            task.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();

            task.Result.Reset();

            bal.IsSet.Should().BeFalse();

            task = bal.WaitAsync();

            task.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();

            task.Result.Set();

            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void WaitAsync_ResetAsync() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.ResetAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void WaitAsync_ResetAsync_Set() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.ResetAsync();

            task1.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void WaitAsync_ResetAsync_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.ResetAsync();

            task1.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void ResetAsync_ResetAsync_Set() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync();

            task1.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void ResetAsync_ResetAsync_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync();

            task1.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void ResetAsync_WaitAsync_Set() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.WaitAsync();

            task1.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void ResetAsync_WaitAsync_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.WaitAsync();

            task1.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void WaitAsync_ResetAsync_Set_Set() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.ResetAsync();

            task1.Result.Set();
            task2.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();

        }

        [Test]
        public void ResetAsync_ResetAsync_Set_Set() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync();

            task1.Result.Set();
            task2.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();}

        [Test]
        public void ResetAsync_ResetAsync_SetIfLast_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync();

            task1.Result.SetIfLast();
            task2.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void WaitAsync_ResetAsync_SetIfLast_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.ResetAsync();

            task1.Result.SetIfLast();
            task2.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void WaitAsync_WaitAsync_SetIfLast_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.WaitAsync();

            task1.Result.Set();
            task2.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void WaitAsync_ResetAsync_Set_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.WaitAsync();
            var task2 = bal.ResetAsync();

            task1.Result.Set();
            task2.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void ResetAsync_ResetAsync_Set_SetIfLast() {
            var bal = new BinaryAsyncLock();
            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync();

            task1.Result.Set();
            task2.Result.SetIfLast();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void ResetAsync_ResetAsyncCanceled_WaitAsync() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var task1 = bal.ResetAsync(cts.Token);
            var task2 = bal.WaitAsync();

            task1.Should().BeCanceled();
            task2.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void ResetAsync_ResetAsync_WaitAsync() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();

            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync(cts.Token);
            var task3 = bal.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void ResetAsync_ResetAsync_WaitAsync_CancelSecond() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();

            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync(cts.Token);
            var task3 = bal.WaitAsync();

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void ResetAsync_ResetAsync_WaitAsync_CancelSecond_SetFirst() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();

            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync(cts.Token);
            var task3 = bal.WaitAsync();

            cts.Cancel();
            task1.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
            bal.IsSet.Should().BeTrue();
        }

        [Test]
        public void SequentalWaitReset_SetToken() {
            var bal = new BinaryAsyncLock(true);
            var task1 = bal.ResetAsync();
            var task2 = bal.WaitAsync();
            var task3 = bal.ResetAsync();
            var task4 = bal.WaitAsync();
            var task5 = bal.ResetAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
            bal.IsSet.Should().BeFalse();

            task1.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
            bal.IsSet.Should().BeFalse();

            task3.Result.Set();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
            task4.Should().BeRanToCompletion();
            task5.Should().BeRanToCompletion();
            bal.IsSet.Should().BeFalse();
        }

        [Test]
        public void CancelWaitAsync_Reset() {
            var bal = new BinaryAsyncLock();
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, 4).Select(i => bal.WaitAsync(cts.Token)).ToList();
            tasks.Should().ContainSingle(t => t.IsCompleted);

            cts.Cancel();
            tasks.Should().OnlyContain(t => t.IsCompleted);
        }
        
        [Test]
        public void CancelWaitAsync_Set() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, 4).Select(i => bal.WaitAsync(cts.Token)).ToList();
            tasks.Should().OnlyContain(t => t.Status == TaskStatus.RanToCompletion);

            cts.Cancel();
            tasks.Should().OnlyContain(t => t.Status == TaskStatus.RanToCompletion);
        }
        
        [Test]
        public void CancelResetAsync_Reset() {
            var bal = new BinaryAsyncLock();
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, 4).Select(i => bal.WaitAsync(cts.Token)).ToList();
            tasks.Should().ContainSingle(t => t.IsCompleted);

            cts.Cancel();
            tasks.Should().OnlyContain(t => t.IsCompleted);
        }
        
        [Test]
        public void CancelResetAsync_Set() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, 4).Select(i => bal.ResetAsync(cts.Token)).ToList();
            tasks.Should().ContainSingle(t => t.IsCompleted);

            cts.Cancel();
            tasks.Should().OnlyContain(t => t.IsCompleted);
        }

        [Test]
        public void CancelEven() {
            var bal = new BinaryAsyncLock(true);
            var cts = new CancellationTokenSource();
            var task1 = bal.ResetAsync();
            var task2 = bal.ResetAsync(cts.Token);
            var task3 = bal.ResetAsync(cts.Token);
            var task4 = bal.WaitAsync();
            var task5 = bal.WaitAsync(cts.Token);

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();

            cts.Cancel();
            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().NotBeCompleted();
            task5.Should().BeCanceled();

            task1.Result.Reset();
            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
            task5.Should().BeCanceled();

            bal.IsSet.Should().BeFalse();
        }
    }
}
