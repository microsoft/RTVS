// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test.Tasks {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class TaskExtensionsTest {
        [Test]
        public void DoNotWait() {
            var expected = new InvalidOperationException();
            var actions = new BlockingCollection<Action>();
            var exceptions = new ConcurrentQueue<Exception>();

            var thread = new Thread(ConsumerThread);
            thread.Start();

            actions.Add(() => ConsumerThreadAction().DoNotWait());

            thread.Join(30000);
            exceptions.Should().Equal(expected);

            void ConsumerThread() {
                foreach (var action in actions.GetConsumingEnumerable()) {
                    var syncContext = SynchronizationContext.Current;
                    try {
                        SynchronizationContext.SetSynchronizationContext(new BlockingCollectionSynchronizationContext(actions));
                        action();
                    } finally {
                        SynchronizationContext.SetSynchronizationContext(syncContext);
                    }
                }
            }

            async Task BackgroundThreadAction() {
                await TaskUtilities.SwitchToBackgroundThread();
                throw expected;
            }

            async Task ConsumerThreadAction() {
                try {
                    Thread.CurrentThread.Should().Be(thread);
                    await BackgroundThreadAction();
                } catch (Exception ex) {
                    exceptions.Enqueue(ex);
                } finally {
                    Thread.CurrentThread.Should().Be(thread);
                    actions.CompleteAdding();
                }
            }
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

        private class BlockingCollectionSynchronizationContext : SynchronizationContext {
            private readonly BlockingCollection<Action> _queue;

            public BlockingCollectionSynchronizationContext(BlockingCollection<Action> queue) {
                _queue = queue;
            }

            public override void Send(SendOrPostCallback d, object state) {
                throw new NotSupportedException();
            }

            public override void Post(SendOrPostCallback d, object state) => _queue.Add(() => d(state));

            public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout) => WaitHelper(waitHandles, waitAll, millisecondsTimeout);

            public override SynchronizationContext CreateCopy() => new BlockingCollectionSynchronizationContext(_queue);
        }
    }
}
