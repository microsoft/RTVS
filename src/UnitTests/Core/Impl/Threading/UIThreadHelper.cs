// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !DESKTOP
using System;
using System.Runtime.Loader;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;

namespace Microsoft.UnitTests.Core.Threading {
    public class UIThreadHelper {
        private static readonly Lazy<UIThreadHelper> LazyInstance = new Lazy<UIThreadHelper>(Create, LazyThreadSafetyMode.ExecutionAndPublication);

        private static UIThreadHelper Create() {
            var uiThreadHelper = new UIThreadHelper();
            var initialized = new ManualResetEventSlim();

            AssemblyLoadContext.Default.Unloading += c => uiThreadHelper.Destroy();
            // We want to maintain an application on a single STA thread
            // set Background so that it won't block process exit.
            var thread = new Thread(RunMainThread) {
                Name = "Main Thread",
                IsBackground = true
            };
            thread.Start(initialized);

            initialized.Wait();
            uiThreadHelper.Invoke(() => {
                uiThreadHelper.Thread = thread;
                uiThreadHelper._syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                uiThreadHelper._taskScheduler = new ControlledTaskScheduler(uiThreadHelper._syncContext);
            });
            return uiThreadHelper;
        }

        public static UIThreadHelper Instance => LazyInstance.Value;

        private readonly AsyncLocal<TestMainThread> _testMainThread;
        private SynchronizationContext _syncContext;
        private ControlledTaskScheduler _taskScheduler;

        private UIThreadHelper() {
            _testMainThread = new AsyncLocal<TestMainThread>();
        }

        public Thread Thread { get; private set; }

        public SynchronizationContext SyncContext => _syncContext;
        public ControlledTaskScheduler TaskScheduler => _taskScheduler;
        public IMainThread MainThread => _testMainThread.Value;
        public IProgressDialog ProgressDialog => _testMainThread.Value;

        internal TestMainThread CreateTestMainThread() {
            if (_testMainThread.Value != null) {
                throw new InvalidOperationException("AsyncLocal<TestMainThread> reentrancy");
            }

            var testMainThread = new TestMainThread(RemoveTestMainThread);
            _testMainThread.Value = testMainThread;
            return testMainThread;
        }

        private void RemoveTestMainThread() => _testMainThread.Value = null;

        public void Invoke(Action action) {
            var exception = CallSafe(action);
            exception?.Throw();
        }

        public Task InvokeAsync(Action action) {
            Invoke(action);
            return Task.CompletedTask;
        }

        public T Invoke<T>(Func<T> action) {
            var result = CallSafe(action);
            result.Exception?.Throw();
            return result.Value;
        }

        public Task<T> InvokeAsync<T>(Func<T> action) {
            Invoke(action);
            return Task.FromResult(default(T));
        }

        private static void RunMainThread(object obj) {
            // Initialization completed
            ((ManualResetEventSlim)obj).Set();
        }

        private void Destroy() {
            var mainThread = Thread;
            Thread = null;

            // If the thread is still alive, allow it to exit normally so the dispatcher can continue to clear pending work items
            // 10 seconds should be enough
            mainThread.Join(10000);
        }

        private static ExceptionDispatchInfo CallSafe(Action action)
            => CallSafe<object>(() => {
                action();
                return null;
            }).Exception;

        private static CallSafeResult<T> CallSafe<T>(Func<T> func) {
            try {
                return new CallSafeResult<T> { Value = func() };
            } catch (Exception e) {
                return new CallSafeResult<T> { Exception = ExceptionDispatchInfo.Capture(e) };
            }
        }

        private class CallSafeResult<T> {
            public T Value { get; set; }
            public ExceptionDispatchInfo Exception { get; set; }
        }
    }
}
#endif
