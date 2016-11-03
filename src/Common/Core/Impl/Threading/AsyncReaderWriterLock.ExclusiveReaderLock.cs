using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    public partial class AsyncReaderWriterLock {
        private class ExclusiveReaderLock : IExclusiveReaderLock {
            private readonly AsyncReaderWriterLock _host;

            public ExclusiveReaderLock(AsyncReaderWriterLock host) {
                _host = host;
            }

            public Task<IAsyncReaderWriterLockToken> WaitAsync(CancellationToken cancellationToken = default(CancellationToken))
                => _host.ExclusiveReaderLockAsync(this, cancellationToken);

            // This field must be accessed only from the Queue instance and only under lock
            // Neither AsyncReaderWriterLock nor ExclusiveReaderLock itself should use them
            public LockSource Tail;
        }

        private class ExclusiveReaderLockSource : LockSource {
            public ExclusiveReaderLock ExclusiveReaderLock { get; }

            public ExclusiveReaderLockSource(AsyncReaderWriterLock host, ExclusiveReaderLock erLock) : base(host, false) {
                ExclusiveReaderLock = erLock;
            }
        }
    }
}