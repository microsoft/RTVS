using System;
using System.Threading;

namespace Microsoft.Common.Core.Disposables
{
    public sealed class CountdownDisposable
    {
        private readonly Action _disposeAction;
        private int _count;

        public int Count => this._count;

        public CountdownDisposable(Action disposeAction = null)
        {
            this._disposeAction = disposeAction ?? (() => { });
        }

        public IDisposable Increment()
        {
            Interlocked.Increment(ref this._count);
            return new DecrementDisposable(this);
        }

        private void Decrement()
        {
            if (Interlocked.Decrement(ref this._count) == 0)
            {
                this._disposeAction();
            }
        }

        private class DecrementDisposable : IDisposable
        {
            private CountdownDisposable _countdownDisposable;

            public DecrementDisposable(CountdownDisposable countdownDisposable)
            {
                this._countdownDisposable = countdownDisposable;
            }

            public void Dispose()
            {
                CountdownDisposable countdown = Interlocked.Exchange(ref this._countdownDisposable, null);
                countdown?.Decrement();
            }
        }
    }
}
