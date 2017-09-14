// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.LanguageServer.Threading {
    internal sealed class MainThread : IMainThread, IDisposable {
        private readonly CancellationTokenSource _ctsExit = new CancellationTokenSource();
        private readonly BufferBlock<Action> _bufferBlock = new BufferBlock<Action>();
        private readonly DisposableBag _disposableBag = DisposableBag.Create<MainThread>();

        public MainThread() {
            _disposableBag.Add(Stop);
            SynchronizationContext = new MainThreadSynchronizationContext(this);
            Thread.Start(this);
        }

        #region IMainThread
        public int ThreadId => Thread.ManagedThreadId;

        public void Post(Action action, CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(o => action(), null, cancellationToken).DoNotWait();
        #endregion

        #region SynchronizationContext
        public SynchronizationContext SynchronizationContext { get; }
        public void Post(Action<object> action, object state) => ExecuteAsync(action, state, CancellationToken.None).DoNotWait();
        public void Send(Action<object> action, object state) => ExecuteAsync(action, state, CancellationToken.None).Wait();
        #endregion

        #region IDisposable
        public void Dispose() => _disposableBag.TryDispose();

        private void Stop() {
            _ctsExit.Cancel();
            _bufferBlock.Post(() => { });
            Thread.Join(1000);
        }
        #endregion

        private Task ExecuteAsync(Action<object> action, object state, CancellationToken cancellationToken)
            => ExecuteAsync(o => {
                action(state);
                return true;
            }, state, cancellationToken);

        private Task<T> ExecuteAsync<T>(Func<object, T> action, object state, CancellationToken cancellationToken) {
            _disposableBag.ThrowIfDisposed();

            if (ThreadId == Thread.CurrentThread.ManagedThreadId) {
                return Task.FromResult(action(state));
            }

            var tcs = new TaskCompletionSource<T>();
            _bufferBlock.Post(() => {
                if (!cancellationToken.IsCancellationRequested) {
                    tcs.TrySetResult(action(state));
                } else {
                    tcs.TrySetCanceled();
                }
            });

            return tcs.Task;
        }

        internal Thread Thread { get; } = new Thread(o => ((MainThread)o).WorkerThread());

        private void WorkerThread() {
            while (true) {
                var action = _bufferBlock.Receive();
                if (_ctsExit.IsCancellationRequested) {
                    break;
                }
                action();
            }
        }

        internal sealed class MainThreadSynchronizationContext : SynchronizationContext {
            private readonly MainThread _mainThread;
            public MainThreadSynchronizationContext(MainThread mainThread) {
                _mainThread = mainThread;
            }

            public override void Post(SendOrPostCallback d, object state) => _mainThread.Post(o => d(o), state);
            public override void Send(SendOrPostCallback d, object state) => _mainThread.Send(o => d(o), state);
        }
    }
}
