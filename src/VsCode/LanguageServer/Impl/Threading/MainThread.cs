// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.LanguageServer.Threading {
    internal sealed class MainThread : IMainThread, IDisposable {
        private sealed class WorkItem {
            public Action<object> Action { get; }
            public object State { get; }
            public CancellationToken CancellationToken { get; }

            public WorkItem(Action<object> action, CancellationToken ct, object state) {
                Action = action;
                CancellationToken = ct;
                State = state;
            }

            public WorkItem(Action action, CancellationToken ct) : this(o => action(), ct, null) { }
        }

        private readonly CancellationTokenSource _ctsExit = new CancellationTokenSource();
        private readonly BufferBlock<WorkItem> _bufferBlock = new BufferBlock<WorkItem>();
        private readonly DisposableBag _disposableBag = DisposableBag.Create<MainThread>();

        public MainThread() {
            _disposableBag.Add(Stop);
            SynchronizationContext = new MainThreadSynchronizationContext(this);
            Thread.Start(this);
        }

        #region IMainThread
        public int ThreadId => Thread.ManagedThreadId;

        public void Post(Action action, CancellationToken cancellationToken = default(CancellationToken)) {
            _disposableBag.ThrowIfDisposed();
            if (ThreadId == Thread.CurrentThread.ManagedThreadId) {
                action();
            } else {
                _bufferBlock.Post(new WorkItem(action, cancellationToken));
            }
        }
        #endregion

        #region SynchronizationContext
        public SynchronizationContext SynchronizationContext { get; }

        public void Post(Action<object> action, object state) {
            _disposableBag.ThrowIfDisposed();
            if (ThreadId == Thread.CurrentThread.ManagedThreadId) {
                action(state);
            } else {
                _bufferBlock.Post(new WorkItem(action, CancellationToken.None, state));
            }
        }
        public void Send(Action<object> action, object state) {
            _disposableBag.ThrowIfDisposed();

            var tcs = new TaskCompletionSource<bool>();
            Post(o => {
                action(state);
                tcs.TrySetResult(true);
            }, state);
            tcs.Task.Wait();
        }
        #endregion

        #region IDisposable

        public void Dispose() => _disposableBag.TryDispose();

        private void Stop() {
            _ctsExit.Cancel();
            _bufferBlock.Post(new WorkItem(() => { }, CancellationToken.None));
            Thread.Join(1000);
        }
        #endregion

        internal Thread Thread { get; } = new Thread(o => ((MainThread) o).WorkerThread());

        private void WorkerThread() {
            while (true) {
                var workItem = _bufferBlock.Receive();
                if (_ctsExit.IsCancellationRequested) {
                    break;
                }
                if (!workItem.CancellationToken.IsCancellationRequested) {
                    workItem.Action(workItem.State);
                }
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
