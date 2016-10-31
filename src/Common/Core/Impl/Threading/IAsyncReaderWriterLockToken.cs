using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    public interface IAsyncReaderWriterLockToken : IDisposable {
        ReentrancyToken Reentrancy { get; }
    }

    public interface IExclusiveReaderLock {
        Task<IAsyncReaderWriterLockToken> WaitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}