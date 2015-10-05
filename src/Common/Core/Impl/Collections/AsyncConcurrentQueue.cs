using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Collections
{
    public class AsyncConcurrentQueue<T> : IReadOnlyCollection<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly ConcurrentQueue<TaskCompletionSource<T>> _pendingDequeue = new ConcurrentQueue<TaskCompletionSource<T>>();
        private int _queueCount;

        public void Enqueue(T item)
        {
            var count = Interlocked.Increment(ref _queueCount);

            if (count > 0)
            {
                _queue.Enqueue(item);
                return;
            }

            TaskCompletionSource<T> pendingTcs;
            if (_pendingDequeue.TryDequeue(out pendingTcs))
            {
                pendingTcs.SetResult(item);
                return;
            }

            var spinWait = new SpinWait();
            while (!_pendingDequeue.TryDequeue(out pendingTcs))
            {
                spinWait.SpinOnce();
            }

            pendingTcs.SetResult(item);
        }

        public bool TryPeek(out T item)
        {
            return _queue.TryPeek(out item);
        }

        public Task<T> DequeueAsync()
        {
            var count = Interlocked.Decrement(ref _queueCount);

            if (count < 0)
            {
                TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
                _pendingDequeue.Enqueue(tcs);
                return tcs.Task;
            }

            T item;
            if (_queue.TryDequeue(out item))
            {
                return Task.FromResult(item);
            }

            var spinWait = new SpinWait();
            while (!_queue.TryDequeue(out item))
            {
                spinWait.SpinOnce();
            }

            return Task.FromResult(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        public int Count => _queue.Count;
    }
}
