// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    /// <summary>
    /// Async lock-free version of ReaderWriterLock with cancellation and reentrancy support. Upgrades aren't supported
    /// </summary>
    /// <remarks>
    /// <para>
    /// This lock prefers writers over readers. If writer lock is requested, all furhter reader locks will be provided only after writer lock is released.
    /// For example, if reader holds a lock and several locks are requested in order Writer-Reader-Writer, they will be provider as Writer-Writer-Reader 
    /// after current lock is released
    /// </para>
    /// <para>
    /// Cancellation affects only unfulfilled lock requests. Canceled requests affect the waiting queue also prefering writers over readers.
    /// For example, if reader holds a lock and several locks are requested in order Writer-Reader-Writer, the waiting queue will be Writer-Writer-Reader,
    /// sp cancelling first writer request will not fulfill following reader request, but instead will put the second waiter in front of the queue.
    /// </para>
    /// <para>
    /// async/await doesn't support reentrancy on the language level, so to support it <see cref="AsyncReaderWriterLock"/> uses <see cref="ReentrancyToken"/> structure.
    /// It can be passed as a method argument in a way similar to the <see cref="CancellationToken"/>.
    /// There are 4 possible types of reentrancy:
    /// <list type="bullet">
    ///    <item>
    ///        <term>Reader requested inside Reader lock</term>
    ///        <description>Reader locks don't block each other, so it will be treated as another reader lock request. The difference is that it will have priority over writer.</description>
    ///    </item>
    ///    <item>
    ///        <term>Reader requested inside Writer lock</term>
    ///        <description>Lock will increment reentrancy counter of the Writer lock, so it will require them both to be released before next lock can be provided</description>
    ///    </item>
    ///    <item>
    ///        <term>Writer requested inside Writer lock</term>
    ///        <description>Lock will increment reentrancy counter of the Writer lock, so it will require them both to be released before next lock can be provided</description>
    ///    </item>
    ///    <item>
    ///        <term>Writer requested inside reader lock</term>
    ///        <description>That is considered an upgrade, which aren't supported right now, so in this case request will be treated as non-reentrant</description>
    ///    </item>
    ///</list>
    /// </para>
    /// </remarks>
    public partial class AsyncReaderWriterLock {
        private static readonly IReentrancyTokenFactory<LockSource> LockTokenFactory;

        static AsyncReaderWriterLock() {
            LockTokenFactory = ReentrancyToken.CreateFactory<LockSource>();
        }

        private readonly Queue _queue;

        public AsyncReaderWriterLock() {
            _queue = new Queue();
        }

        public Task<IAsyncReaderWriterLockToken> ReaderLockAsync(CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var task = ReentrantOrCanceled(false, cancellationToken, reentrancyToken);
            if (task != null) {
                return task;
            }

            var source = new LockSource(this, false);
            _queue.AddReader(source, out var isAddedAfterWriter);
            if (isAddedAfterWriter) {
                source.RegisterCancellation(cancellationToken);
            } else {
                source.Release();
            }

            return source.Task;
        }

        public Task<IAsyncReaderWriterLockToken> WriterLockAsync(CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var task = ReentrantOrCanceled(true, cancellationToken, reentrancyToken);
            if (task != null) {
                return task;
            }

            var source = new LockSource(this, true);
            _queue.AddWriter(source, out var isFirstWriter);
            if (isFirstWriter) {
                source.Release();
            } else {
                source.RegisterCancellation(cancellationToken);
            }

            return source.Task;
        }

        public IExclusiveReaderLock CreateExclusiveReaderLock() => new ExclusiveReaderLock(this);

        private Task<IAsyncReaderWriterLockToken> ExclusiveReaderLockAsync(ExclusiveReaderLock erLock, CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<IAsyncReaderWriterLockToken>(cancellationToken);
            }

            var source = new ExclusiveReaderLockSource(this, erLock);
            _queue.AddExclusiveReader(source, out var isAddedAfterWriterOrExclusiveReader);
            if (isAddedAfterWriterOrExclusiveReader) {
                source.RegisterCancellation(cancellationToken);
            } else {
                source.Release();
            }

            return source.Task;
        }


        private static Task<IAsyncReaderWriterLockToken> ReentrantOrCanceled(bool writerOnly, CancellationToken cancellationToken, ReentrancyToken reentrancyToken) {
            var source = LockTokenFactory.GetSource(reentrancyToken);
            if (source != null && source.TryReenter(writerOnly, out var task)) {
                return task;
            }

            return cancellationToken.IsCancellationRequested ? Task.FromCanceled<IAsyncReaderWriterLockToken>(cancellationToken) : null;
        }

        private void Remove(LockSource lockSource) {
            var sourcesToRelease = _queue.Remove(lockSource);
            foreach (var source in sourcesToRelease) {
                source.Release();
            }
        }
    }
}