using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionTransaction : IDisposable {
        bool IsSessionDisposed { get; }

        /// <summary>
        /// First step of the transaction. Acquires lock that prevents switch, reconnect 
        /// and other types of initialization from being called concurrently.
        /// </summary>
        Task AcquireLockAsync(CancellationToken cancellationToken);
    }
}