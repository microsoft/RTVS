// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Tasks {
    [ExcludeFromCodeCoverage]
    public class TaskUtilitiesTest {
        [Test]
        public async Task CreateCanceled() {
            var exception = new CustomOperationCanceledException();
            var task = TaskUtilities.CreateCanceled<int>(exception);

            task.Status.Should().Be(TaskStatus.Canceled);
            task.IsCanceled.Should().Be(true);

            Func<Task<int>> f = async () => await task;
            await f.ShouldThrowAsync<CustomOperationCanceledException>();
        }

        [Test]
        public async Task WhenAllCancelOnFailure_Array() {
            var index = 0;

            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(200, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                Interlocked.Exchange(ref index, 2);
                throw new InvalidOperationException("2");
            };

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(function1, function2);
            (await f.ShouldThrowAsync<InvalidOperationException>()).WithMessage("2");
            index.Should().Be(2);
        }

        [Test]
        public async Task WhenAllCancelOnFailure_NoCancellation() {
            var index = 0;

            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(200, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                Interlocked.Exchange(ref index, 2);
                throw new InvalidOperationException("2");
            };

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, CancellationToken.None);
            (await f.ShouldThrowAsync<InvalidOperationException>()).WithMessage("2");
            index.Should().Be(2);
        }

        [Test]
        public async Task WhenAllCancelOnFailure_AlreadyCancellation() {
            var index = 0;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(200, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                Interlocked.Exchange(ref index, 2);
                throw new InvalidOperationException("2");
            };

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            await f.ShouldThrowAsync<OperationCanceledException>();
            index.Should().Be(0);
        }

        [Test]
        public async Task WhenAllCancelOnFailure_FailureThenCancellation() {
            var index = 0;

            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(350, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                Interlocked.Exchange(ref index, 2);
                throw new InvalidOperationException("2");
            };

            var cts = new CancellationTokenSource(200);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            (await f.ShouldThrowAsync<InvalidOperationException>()).WithMessage("2");
            index.Should().Be(2);
        }

        [Test]
        public async Task WhenAllCancelOnFailure_CancellationThenFailure() {
            var index = 0;

            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(350, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(200, ct);
                Interlocked.Exchange(ref index, 2);
                throw new InvalidOperationException("2");
            };

            var cts = new CancellationTokenSource(50);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            await f.ShouldThrowAsync<OperationCanceledException>();
            index.Should().Be(0);
        }

        [Test]
        public async Task WhenAllCancelOnFailure_FailureOnCancellation() {
            var index = 0;

            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(200, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                Interlocked.Exchange(ref index, 2);
                throw new OperationCanceledException("2");
            };

            var cts = new CancellationTokenSource(200);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            (await f.ShouldThrowAsync<OperationCanceledException>()).WithMessage("2");
            index.Should().Be(2);
        }

        [Test]
        public async Task WhenAllCancelOnFailure_SyncCompleted() {
            Func<CancellationToken, Task> function1 = ct => Task.CompletedTask;
            Func<CancellationToken, Task> function2 = ct => Task.Delay(200, ct);

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(function1, function2);
            await f.ShouldNotThrowAsync();
        }

        [Test]
        public async Task WhenAllCancelOnFailure_SyncFailure() {
            Func<CancellationToken, Task> function1 = 
                ct => Task.FromException<InvalidOperationException>(new InvalidOperationException());

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(200, ct);
                ct.ThrowIfCancellationRequested();
            };

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(function1, function2);
            await f.ShouldThrowAsync<InvalidOperationException>();
        }
        
        [Test]
        public async Task WhenAllCancelOnFailure_SyncCompletedThenCancellationThenFailure() {
            var index = 0;

            Func<CancellationToken, Task> function1 = ct => Task.CompletedTask;

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(350, ct);
                Interlocked.Exchange(ref index, 1);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function3 = async ct => {
                await Task.Delay(200, ct);
                Interlocked.Exchange(ref index, 2);
                throw new InvalidOperationException("2");
            };

            var cts = new CancellationTokenSource(50);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new[] { function1, function2, function3 }, cts.Token);
            await f.ShouldThrowAsync<OperationCanceledException>();
            index.Should().Be(0);
        }

        private class CustomOperationCanceledException : OperationCanceledException { }
    }
}
