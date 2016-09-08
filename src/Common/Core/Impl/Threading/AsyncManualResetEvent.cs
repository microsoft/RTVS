using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    /// <summary>
    /// Async version of the ManualResetEvent
    /// </summary>
    public class AsyncManualResetEvent {
        private TaskCompletionSource<bool> _tcs;
        public Task WaitAsync() => _tcs.Task;
        public void Set() => _tcs.TrySetResult(true);

        public Task WaitAsync(CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) {
                return Task.FromCanceled(cancellationToken);
            }

            var cancellationTcs = new TaskCompletionSource<bool>();
            var registration = cancellationToken.Register(CancelTcs, cancellationTcs);
            return Task.WhenAny(_tcs.Task.ContinueWith(UnsubscribeCancellationTcs, registration, TaskContinuationOptions.ExecuteSynchronously), cancellationTcs.Task);
        }

        public AsyncManualResetEvent() {
            _tcs = new TaskCompletionSource<bool>();
        }

        public void Reset() {
            while (true) {
                var tcs = _tcs;
                if (!tcs.Task.IsCompleted) {
                    return;
                }

                if (Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(), tcs) == tcs) {
                    return;
                }
            }
        }

        private static void UnsubscribeCancellationTcs(Task<bool> task, object arg2) {
            var registration = (CancellationTokenRegistration) arg2;
            registration.Dispose();
        }

        private static void CancelTcs(object obj) {
            var tcs = (TaskCompletionSource<bool>) obj;
            tcs.TrySetCanceled();
        }
    }
}