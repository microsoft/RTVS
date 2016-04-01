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
        public async Task Release() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                var isSet = await bal.WaitAsync();
                if (!isSet) {
                    Interlocked.Increment(ref count);
                    bal.Release();
                }
            });

            count.Should().Be(1);
        }

        [Test]
        public async Task Reset() {
            var bal = new BinaryAsyncLock();
            var count = 0;
            await ParallelTools.InvokeAsync(4, async i => {
                try {
                    var isSet = await bal.WaitAsync();
                    if (!isSet) {
                        await Task.Delay(50);
                        bal.Reset();
                    }
                } catch (OperationCanceledException) {
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(3);
        }
    }
}
