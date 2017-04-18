// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.Common.Core.Tasks;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Tasks {
    [ExcludeFromCodeCoverage]
    public class TaskCompletionSourceExTest {
        [Test]
        public void TrySetResult() {
            var tcs = new TaskCompletionSourceEx<int>();
            var count = 0;
            ParallelTools.Invoke(8, i => {
                var isSet = tcs.TrySetResult(i);
                if (isSet) {
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(1);
        }

        [Test]
        public void TrySetCanceled() {
            var tcs = new TaskCompletionSourceEx<int>();
            var count = 0;
            ParallelTools.Invoke(8, i => {
                var isSet = tcs.TrySetCanceled();
                if (isSet) {
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(1);
        }

        [Test]
        public void TrySetException() {
            var tcs = new TaskCompletionSourceEx<int>();
            var count = 0;
            ParallelTools.Invoke(8, i => {
                var isSet = tcs.TrySetException(new Exception());
                if (isSet) {
                    Interlocked.Increment(ref count);
                }
            });

            count.Should().Be(1);
        }
    }
}