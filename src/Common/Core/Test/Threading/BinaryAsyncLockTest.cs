// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Threading {
    public class BinaryAsyncLockTest {
        [Test]
        public async Task WaitAsync_Release() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var isSet = await bal.WaitAsync();
                if (!isSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                    bal.Release();
                }
            });

            count.Should().Be(1);
        }

        [Test]
        public async Task WaitAsync_Released() {
            var bal = new BinaryAsyncLock();
            bal.Release();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var isSet = await bal.WaitAsync();
                if (!isSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(0);
        }

        [Test]
        public async Task WaitAsyncIfLocked_Release_Unlocked() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var isSet = await bal.WaitIfLockedAsync();
                if (!isSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(4);
        }

        [Test]
        public async Task WaitAsyncIfLocked_Released() {
            var bal = new BinaryAsyncLock();
            bal.Release();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var isSet = await bal.WaitIfLockedAsync();
                if (!isSet) {
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
                var isSet = await bal.WaitAsync();
                if (!isSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                    bal.Reset();
                }
            });

            count.Should().Be(4);
        }

        [Test]
        public async Task WaitAsyncIfLocked_Reset_Unlocked() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var isSet = await bal.WaitIfLockedAsync();
                if (!isSet) {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                    bal.Reset();
                }
            });

            count.Should().Be(4);
        }
    }
}
