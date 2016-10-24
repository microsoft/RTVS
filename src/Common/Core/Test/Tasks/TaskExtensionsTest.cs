// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test.Tasks {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class TaskExtensionsTest {
        [Test]
        public async Task DoNotWait() {
            var expected = new InvalidOperationException();

            Func<Task> backgroundThreadAction = async () => {
                await TaskUtilities.SwitchToBackgroundThread();
                throw expected;
            };

            Func<Task> uiThreadAction = async () => {
                try {
                    Thread.CurrentThread.Should().Be(UIThreadHelper.Instance.Thread);
                    await backgroundThreadAction();
                } finally {
                    Thread.CurrentThread.Should().Be(UIThreadHelper.Instance.Thread);
                }
            };

            var exceptionTask = UIThreadHelper.Instance.WaitForNextExceptionAsync();

            UIThreadHelper.Instance.Invoke(() => uiThreadAction().DoNotWait());
            var actual = await exceptionTask;
            actual.Should().Be(expected);
        }

        [Test]
        public void SilenceException() {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task.SilenceException<SystemException>();
            tcs.SetException(new InvalidOperationException());
            Func<Task> f = async () => await task;
            f.ShouldNotThrow();
            task.IsCanceled.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();
        }

        [Test]
        public void SilenceException_Sequence() {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task
                .SilenceException<InvalidOperationException>()
                .SilenceException<NullReferenceException>();

            tcs.SetException(new Exception [] { new NullReferenceException(), new InvalidOperationException() });
            Func<Task> f = async () => await task;
            f.ShouldNotThrow();
            task.IsCanceled.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();
        }

        [Test]
        public void SilenceException_Faulted() {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task.SilenceException<NullReferenceException>();
            tcs.SetException(new InvalidOperationException());
            Func<Task> f = async () => await task;
            f.ShouldThrow<InvalidOperationException>();
            task.IsCanceled.Should().BeFalse();
            task.IsFaulted.Should().BeTrue();
        }

        [Test]
        public void SilenceException_SequenceFaulted() {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task
                .SilenceException<NotSupportedException>()
                .SilenceException<NullReferenceException>();

            tcs.SetException(new Exception[] { new NullReferenceException(), new InvalidOperationException() });
            Func<Task> f = async () => await task;
            f.ShouldThrowExactly<InvalidOperationException>();
            task.IsCanceled.Should().BeFalse();
            task.IsFaulted.Should().BeTrue();
        }

        [Test]
        public void SilenceException_Canceled() {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task.SilenceException<SystemException>();
            tcs.SetCanceled();
            Func<Task> f = async () => await task;
            f.ShouldThrow<OperationCanceledException>();
            task.IsCanceled.Should().BeTrue();
            task.IsFaulted.Should().BeFalse();
        }
    }
}
