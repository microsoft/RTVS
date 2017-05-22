using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    public partial class AsyncReaderWriterLock {
        [DebuggerDisplay("{IsWriter ? \"Writer\" : \"Reader\"}: {_tcs.Task.IsCompleted ? (_tcs.Task.IsCanceled ? \"Canceled\" : \"Released\") : \"Wait\"}")]
        private class LockSource {
            private readonly AsyncReaderWriterLock _host;
            private readonly TaskCompletionSource<IAsyncReaderWriterLockToken> _tcs;
            private CancellationTokenRegistration _cancellationTokenRegistration;
            private int _reentrancyCount;

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

            public void RegisterCancellation(CancellationToken cancellationToken) => _cancellationTokenRegistration = cancellationToken.Register(Cancel);

            public bool TryRemoveFromQueue() {
                var count = Interlocked.Decrement(ref _reentrancyCount);
                if (count > 0) {
                    return false;
                }

                _host.Remove(this);
                return true;
            }
            
            private void Cancel() {
                if (_tcs.TrySetCanceled()) {
                    _cancellationTokenRegistration.Dispose();
                    TryRemoveFromQueue();
                }
            }

            // These two fields must be accessed only from the Queue instance and only under lock
            // Neither AsyncReaderWriterLock nor LockSource itself should use them
            public LockSource Next;
            public LockSource Previous;      
        }
    }
}