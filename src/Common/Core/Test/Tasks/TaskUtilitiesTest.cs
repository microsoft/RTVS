// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Tasks {
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
            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(100, ct);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                throw new InvalidOperationException("2");
            };

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(function1, function2);
            (await f.ShouldThrowAsync<InvalidOperationException>()).WithMessage("2");
        }

        [Test]
        public async Task WhenAllCancelOnFailure_NoCancellation() {
            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(100, ct);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                throw new InvalidOperationException("2");
            };

            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, CancellationToken.None);
            (await f.ShouldThrowAsync<InvalidOperationException>()).WithMessage("2");
        }

        [Test]
        public async Task WhenAllCancelOnFailure_FailureThenCancellation() {
            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(150, ct);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                throw new InvalidOperationException("2");
            };

            var cts = new CancellationTokenSource(100);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            (await f.ShouldThrowAsync<InvalidOperationException>()).WithMessage("2");
        }

        [Test]
        public async Task WhenAllCancelOnFailure_CancellationThenFailure() {
            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(150, ct);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(100, ct);
                throw new InvalidOperationException("2");
            };

            var cts = new CancellationTokenSource(50);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            await f.ShouldThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task WhenAllCancelOnFailure_FailureOnCancellation() {
            Func<CancellationToken, Task> function1 = async ct => {
                await Task.Delay(100, ct);
                throw new InvalidOperationException("1");
            };

            Func<CancellationToken, Task> function2 = async ct => {
                await Task.Delay(50, ct);
                throw new OperationCanceledException("2");
            };

            var cts = new CancellationTokenSource(100);
            Func<Task> f = () => TaskUtilities.WhenAllCancelOnFailure(new [] { function1, function2 }, cts.Token);
            (await f.ShouldThrowAsync<OperationCanceledException>()).WithMessage("2");
        }

        private class CustomOperationCanceledException : OperationCanceledException { }
    }
}
