using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Tasks {
    public sealed class EventTaskSource<T> : EventTaskSource<T, EventArgs> {
        public EventTaskSource(Action<T, EventHandler> subscribe, Action<T, EventHandler> unsubscribe)
            : base ((t, h) => subscribe(t, (o, e) => h(o, e)), (t, h) => unsubscribe(t, (o, e) => h(o, e))) {
        }
    }

    public class EventTaskSource<T, TEventArgs> {
        private readonly Action<T, EventHandler<TEventArgs>> _subscribe;
        private readonly Action<T, EventHandler<TEventArgs>> _unsubscribe;

        public EventTaskSource(Action<T, EventHandler<TEventArgs>> subscribe, Action<T, EventHandler<TEventArgs>> unsubscribe) {
            _subscribe = subscribe;
            _unsubscribe = unsubscribe;
        }

        public Task<TEventArgs> Create(T instance, CancellationToken cancellationToken = default(CancellationToken)) {
            var tcs = new TaskCompletionSource<TEventArgs>();
            var reference = new HandlerReference(instance, tcs, _unsubscribe);
            if (cancellationToken != CancellationToken.None) {
                cancellationToken.Register(reference.Cancel);
            }
            _subscribe(instance, reference.Handler);
            return tcs.Task;
        }

        private class HandlerReference {
            private T _instance;
            private TaskCompletionSource<TEventArgs> _tcs;
            private Action<T, EventHandler<TEventArgs>> _unsubscribe;

            public HandlerReference(T instance, TaskCompletionSource<TEventArgs> tcs, Action<T, EventHandler<TEventArgs>> unsubscribe) {
                _instance = instance;
                _tcs = tcs;
                _unsubscribe = unsubscribe;
            }

            public void Handler(object sender, TEventArgs e) {
                var tcs = Unsubscribe();
                tcs?.SetResult(e);
            }

            public void Cancel() {
                var tcs = Unsubscribe();
                tcs?.SetCanceled();
            }

            private TaskCompletionSource<TEventArgs> Unsubscribe() {
                var tcs = Interlocked.Exchange(ref _tcs, null);
                if (tcs == null) {
                    return null;
                }

                var instance = _instance;
                var unsubscribe = _unsubscribe;
                _instance = default(T);
                _unsubscribe = null;
                unsubscribe(instance, Handler);
                return tcs;
            }
        }
    }
}
