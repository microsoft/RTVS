// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    public class AsyncManualResetEventTest {
        [Test]
        public async Task WaitAsync_CancellationToken_Canceled() {
            var amre = new AsyncManualResetEvent();
            Func<Task> f = () => amre.WaitAsync(new CancellationToken(true));
            await f.ShouldThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task WaitAsync_CancellationToken_Cancel() {
            var amre = new AsyncManualResetEvent();
            var cts = new CancellationTokenSource();

            var task = amre.WaitAsync(cts.Token);
            task.Should().NotBeCompleted();

            cts.Cancel();
            await Task.Delay(10);

            task.Should().BeCanceled();
        }

        [Test]
        public async Task WaitAsync_CancellationToken_CancelAfterSet() {
            var amre = new AsyncManualResetEvent();
            var cts = new CancellationTokenSource();

            var task = amre.WaitAsync(cts.Token);
            task.Should().NotBeCompleted();

            amre.Set();
            cts.Cancel();
            await Task.Delay(10);

            task.Should().BeRanToCompletion();
        }

        [Test]
        public async Task WaitAsync_CancellationToken_CancelAfterReset() {
            var amre = new AsyncManualResetEvent();
            var cts = new CancellationTokenSource();

            var task = amre.WaitAsync(cts.Token);
            task.Should().NotBeCompleted();

            amre.Reset();
            cts.Cancel();
            await Task.Delay(10);

            task.Should().BeCanceled();
        }

        [Test]
        public async Task WaitAsync_CancellationToken_CancelAfterResetAfterSet() {
            var amre = new AsyncManualResetEvent();
            var cts = new CancellationTokenSource();

            var task = amre.WaitAsync(cts.Token);
            task.Should().NotBeCompleted();

            amre.Set();
            amre.Reset();

            cts.Cancel();
            await Task.Delay(10);

            task.Should().BeRanToCompletion();
        }

        [Test]
        public async Task WaitAsync_CancellationToken_SetWithNotCanceled() {
            var amre = new AsyncManualResetEvent();
            var cts = new CancellationTokenSource();

            var task = amre.WaitAsync(CancellationToken.None);
            var canceledTask = amre.WaitAsync(cts.Token);
            task.Should().NotBeCompleted();
            canceledTask.Should().NotBeCompleted();

            cts.Cancel();
            amre.Set();

            await Task.Delay(10);

            task.Should().BeRanToCompletion();
            canceledTask.Should().BeCanceled();
        }
    }
}