using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;

namespace Microsoft.UnitTests.Core.Threading {
    public class TestMainThread : IProgressDialog, IMainThread, IDisposable {
        private readonly Action _onDispose;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly AsyncLocal<BlockingLoop> _blockingLoop = new AsyncLocal<BlockingLoop>();

        public TestMainThread(Action onDispose) {
            _onDispose = onDispose;
        }

        public int ThreadId => UIThreadHelper.Instance.Thread.ManagedThreadId;

        public void Dispose() => _onDispose();

        public void Post(Action action, CancellationToken cancellationToken) {
            var bl = _blockingLoop.Value;
            if (bl != null) {
                bl.Post(action);
            } else {
                var token = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token;
                var registration = token.Register(action, false);
                var task = UIThreadHelper.Instance.InvokeAsync(action);
                registration.UnregisterOnCompletion(task);
            }
        }

        public void Show(Func<CancellationToken, Task> method, string waitMessage, int delayToShowDialogMs = 0)
            => BlockUntilCompleted(() => method(CancellationToken.None));

        public TResult Show<TResult>(Func<CancellationToken, Task<TResult>> method, string waitMessage, int delayToShowDialogMs = 0)
            => BlockUntilCompleted(() => method(CancellationToken.None));

        public void Show(Func<IProgress<ProgressDialogData>, CancellationToken, Task> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0)
            => BlockUntilCompleted(() => method(new Progress<ProgressDialogData>(), CancellationToken.None));

        public T Show<T>(Func<IProgress<ProgressDialogData>, CancellationToken, Task<T>> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0)
            => BlockUntilCompleted(() => method(new Progress<ProgressDialogData>(), CancellationToken.None));

        private void BlockUntilCompleted(Func<Task> func) => BlockUntilCompletedImpl(func);

        private TResult BlockUntilCompleted<TResult>(Func<Task<TResult>> func) {
            var task = BlockUntilCompletedImpl(func);
            return ((Task<TResult>)task).Result;
        }

        private Task BlockUntilCompletedImpl(Func<Task> func) {
            if (UIThreadHelper.Instance.Thread != Thread.CurrentThread) {
                try {
                    var task = func();
                    task.GetAwaiter().GetResult();
                    return task;
                } catch (OperationCanceledException ex) {
                    return TaskUtilities.CreateCanceled(ex);
                } catch (Exception ex) {
                    return Task.FromException(ex);
                }
            }

            var sc = SynchronizationContext.Current;
            var blockingLoopSynchronizationContext = new BlockingLoopSynchronizationContext(UIThreadHelper.Instance, sc);
            SynchronizationContext.SetSynchronizationContext(blockingLoopSynchronizationContext);
            var bl = new BlockingLoop(func, sc);
            try {
                _blockingLoop.Value = bl;
                bl.Start();
            } finally {
                _blockingLoop.Value = null;
                SynchronizationContext.SetSynchronizationContext(sc);
            }

            return bl.Task;
        }

        public void CancelPendingTasks() => _cts.Cancel();

        private class BlockingLoop {
            private readonly Func<Task> _func;
            private readonly SynchronizationContext _previousSyncContext;
            private readonly AutoResetEvent _are;
            private readonly ConcurrentQueue<Action> _actions;

            public Task Task { get; private set; }

            public BlockingLoop(Func<Task> func, SynchronizationContext previousSyncContext) {
                _func = func;
                _previousSyncContext = previousSyncContext;
                _are = new AutoResetEvent(false);
                _actions = new ConcurrentQueue<Action>();
            }

            public void Start() {
                Task = _func();
                Task.ContinueWith(Complete);
                while (!Task.IsCompleted) {
                    _are.WaitOne();
                    ProcessQueue();
                }
            }

            // TODO: Add support for cancellation token
            public void Post(Action action) {
                _actions.Enqueue(action);
                _are.Set();
                if (Task.IsCompleted) {
                    _previousSyncContext.Post(c => ProcessQueue(), null);
                }
            }

            private void Complete(Task task) => _are.Set();

            private void ProcessQueue() {
                while (_actions.TryDequeue(out var action)) {
                    action();
                }
            }
        }

        private class BlockingLoopSynchronizationContext : SynchronizationContext {
            private readonly UIThreadHelper _threadHelper;
            private readonly SynchronizationContext _innerSynchronizationContext;

            public BlockingLoopSynchronizationContext(UIThreadHelper threadHelper, SynchronizationContext innerSynchronizationContext) {
                _threadHelper = threadHelper;
                _innerSynchronizationContext = innerSynchronizationContext;
            }

            public override void Send(SendOrPostCallback d, object state)
                => _innerSynchronizationContext.Send(d, state);

            public override void Post(SendOrPostCallback d, object state) {
                var bl = ((TestMainThread)_threadHelper.MainThread)._blockingLoop.Value;
                if (bl != null) {
                    bl.Post(() => d(state));
                } else {
                    _innerSynchronizationContext.Post(d, state);
                }
            }

            public override SynchronizationContext CreateCopy()
                => new BlockingLoopSynchronizationContext(_threadHelper, _innerSynchronizationContext);
        }
    }
}