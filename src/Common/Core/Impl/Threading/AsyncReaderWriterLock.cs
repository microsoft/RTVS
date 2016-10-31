// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Linq;
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

        private readonly Queue<LockSource> _queue;

        public AsyncReaderWriterLock() {
            _queue = new Queue<LockSource>();
        }

        public Task<IAsyncReaderWriterLockToken> ReaderLockAsync(CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {
            Task<IAsyncReaderWriterLockToken> task;
            var source = LockTokenFactory.GetSource(reentrancyToken);
            if (source != null && source.TryReenter(false, out task)) {
                return task;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<IAsyncReaderWriterLockToken>(cancellationToken);
            }

            source = new LockSource(this, false);
            bool isAddedAfterWriter;
            _queue.AddReader(source, out isAddedAfterWriter);
            if (isAddedAfterWriter) {
                source.RegisterCancellation(cancellationToken);
            } else {
                source.Release();
            }

            return source.Task;
        }

        public Task<IAsyncReaderWriterLockToken> WriterLockAsync(CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {
            Task<IAsyncReaderWriterLockToken> task;
            var writerFromToken = LockTokenFactory.GetSource(reentrancyToken);
            if (writerFromToken != null && writerFromToken.TryReenter(true, out task)) {
                return task;
            }

            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled<IAsyncReaderWriterLockToken>(cancellationToken);
            }

            var source = new LockSource(this, true);
            _queue.AddWriter(source);
            if (_queue.GetFirstAsWriter() == source && !_queue.GetFirstReaders().Any()) {
                source.Release();
            } else {
                source.RegisterCancellation(cancellationToken);
            }

            return source.Task;
        }

        private void RemoveFromQueue(LockSource lockSource) {
            _queue.Remove(lockSource);
            var writer = _queue.GetFirstAsWriter();
            if (writer != null) {
                writer.Release();
            } else {
                foreach (var reader in _queue.GetFirstReaders()) {
                    reader.Release();
                }
            }
        }

        [DebuggerDisplay("{IsWriter ? \"Writer\" : \"Reader\"}, IsReleased = {_tcs.Task.IsCompleted}, IsPendingRemoval = {IsPendingRemoval}")]
        private class LockSource : IQueueItem {
            private readonly AsyncReaderWriterLock _host;
            private readonly TaskCompletionSource<IAsyncReaderWriterLockToken> _tcs;
            private CancellationTokenRegistration _cancellationTokenRegistration;
            private int _reentrancyCount;
            private int _removed;
            private IQueueItem _next;

            public Task<IAsyncReaderWriterLockToken> Task => _tcs.Task;
            public bool IsWriter { get; }

            public LockSource(AsyncReaderWriterLock host, bool isWriter) {
                _host = host;
                _tcs = new TaskCompletionSource<IAsyncReaderWriterLockToken>();
                _reentrancyCount = 1;
                IsWriter = isWriter;
            }

            public void Release() {
                if (_tcs.TrySetResult(new Token(this))) { 
                    _cancellationTokenRegistration.Dispose();
                }
            }

            public bool TryReenter(bool writerOnly, out Task<IAsyncReaderWriterLockToken> task) {
                if (!IsWriter && writerOnly) {
                    task = null;
                    return false;
                }

                while (true) {
                    var count = _reentrancyCount;
                    if (count == 0) {
                        task = null;
                        return false;
                    }

                    if (Interlocked.CompareExchange(ref _reentrancyCount, count + 1, count) == count) {
                        task = _tcs.Task;
                        return true;
                    }
                }
            }

            public void RegisterCancellation(CancellationToken cancellationToken) {
                _cancellationTokenRegistration = cancellationToken.Register(Cancel);
            }

            public bool TryRemoveFromQueue() {
                var count = Interlocked.Decrement(ref _reentrancyCount);
                if (count > 0) {
                    return false;
                }

                _host.RemoveFromQueue(this);
                return true;
            }
            
            private void Cancel() {
                if (_tcs.TrySetCanceled()) {
                    _cancellationTokenRegistration.Dispose();
                    TryRemoveFromQueue();
                }
            }

            public bool IsPendingRemoval => _reentrancyCount == 0;
            public bool IsRemoved => _removed == 1;
            public IQueueItem Next => _next;

            public void MarkRemoved() => Interlocked.Exchange(ref _removed, 1);
            public IQueueItem TrySetNext(IQueueItem value, IQueueItem comparand) => Interlocked.CompareExchange(ref _next, value, comparand);
        }

        private class Token : IAsyncReaderWriterLockToken {
            private LockSource _lockSource;
            public ReentrancyToken Reentrancy { get; }

            public Token(LockSource lockSource) {
                _lockSource = lockSource;
                Reentrancy = LockTokenFactory.Create(lockSource);
            }

            public void Dispose() {
                if (_lockSource.TryRemoveFromQueue()) {
                    _lockSource = null;
                }
            }
        }
    }
}