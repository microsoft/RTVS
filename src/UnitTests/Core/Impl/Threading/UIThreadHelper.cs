// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.Threading {
    [ExcludeFromCodeCoverage]
    public class UIThreadHelper {
        [DllImport("ole32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int OleInitialize(IntPtr value);

        private static readonly Lazy<UIThreadHelper> LazyInstance = new Lazy<UIThreadHelper>(Create, LazyThreadSafetyMode.ExecutionAndPublication);

        private static UIThreadHelper Create() {
            UIThreadHelper uiThreadHelper = new UIThreadHelper();
            ManualResetEventSlim initialized = new ManualResetEventSlim();

            AppDomain.CurrentDomain.DomainUnload += uiThreadHelper.Destroy;
            AppDomain.CurrentDomain.ProcessExit += uiThreadHelper.Destroy;

            // We want to maintain an application on a single STA thread
            // set Background so that it won't block process exit.
            Thread thread = new Thread(uiThreadHelper.RunMainThread) { Name = "WPF Dispatcher Thread" };
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start(initialized);

            initialized.Wait();
            uiThreadHelper.Invoke(() => {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(uiThreadHelper._frame.Dispatcher));

                uiThreadHelper._thread = thread;
                uiThreadHelper._syncContext = SynchronizationContext.Current;
                uiThreadHelper._taskScheduler = new ControlledTaskScheduler(uiThreadHelper._syncContext);
            });
            return uiThreadHelper;
        }

        public static UIThreadHelper Instance => LazyInstance.Value;

        private Thread _thread;
        private DispatcherFrame _frame;
        private Application _application;
        private SynchronizationContext _syncContext;
        private ControlledTaskScheduler _taskScheduler;

        private UIThreadHelper() {
        }

        public Thread Thread => _thread;
        public SynchronizationContext SyncContext => _syncContext;
        public ControlledTaskScheduler TaskScheduler => _taskScheduler;

        public void Invoke(Action action) {
            ExceptionDispatchInfo exception = _thread == Thread.CurrentThread
               ? CallSafe(action)
               : _application.Dispatcher.Invoke(() => CallSafe(action));

            exception?.Throw();
        }

        public async Task InvokeAsync(Action action, CancellationToken cancellationToken = default(CancellationToken)) {
            ExceptionDispatchInfo exception;
            if (_thread == Thread.CurrentThread) {
                exception = CallSafe(action);
            } else {
                exception = await _application.Dispatcher.InvokeAsync(() => CallSafe(action), DispatcherPriority.Normal, cancellationToken);
            }

            exception?.Throw();
        }

        public void WaitForUpcomingTasks(IDataflowBlock block, int ms = 1000) {
            TaskScheduler.WaitForUpcomingTasks(ms);
            if (block.Completion.IsFaulted && block.Completion.Exception != null) {
                throw block.Completion.Exception;
            }
        }

        public T Invoke<T>(Func<T> action) {
            var result = _thread == Thread.CurrentThread
               ? CallSafe(action)
               : _application.Dispatcher.Invoke(() => CallSafe(action));

            result.Exception?.Throw();

            return result.Value;
        }

        public async Task<T> InvokeAsync<T>(Func<T> action) {
            CallSafeResult<T> result;
            if (_thread == Thread.CurrentThread) {
                result = CallSafe(action);
            } else {
                result = await _application.Dispatcher.InvokeAsync(() => CallSafe(action));
            }

            result.Exception?.Throw();

            return result.Value;
        }

        public async Task<Exception> WaitForNextExceptionAsync(CancellationToken cancellationToken = default (CancellationToken)) {
            var args = await EventTaskSources.Dispatcher.UnhandledException.Create(_application.Dispatcher, e => e.Handled = true, cancellationToken);
            return args.Exception;
        }

        public void DoEvents() {
            if (TaskUtilities.IsOnBackgroundThread()) {
                DoEventsAsync().WaitAndUnwrapExceptions();
                return;
            }

            DispatcherFrame frame = new DispatcherFrame();
            _application.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public void DoEvents(int ms) {
            if (ms < 0) {
                throw new ArgumentOutOfRangeException(nameof(ms));
            }

            if (ms == 0) {
                DoEvents();
            } else if (TaskUtilities.IsOnBackgroundThread()) {
                Task.Delay(ms)
                    .ContinueWith(t => DoEventsAsync())
                    .Wait();
            } else {
                DispatcherFrame frame = new DispatcherFrame();
                Task.Delay(ms)
                    .ContinueWith(t => _application.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame));
                Dispatcher.PushFrame(frame);
            }
        }

        private object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        public Task DoEventsAsync() {
            return TaskUtilities.IsOnBackgroundThread()
                ? _application.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background).Task
                : Task.Run(DoEventsAsync);
        }

        private void RunMainThread(object obj) {
            if (Application.Current != null) {
                // Need to be on our own sta thread
                Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);

                if (Application.Current != null) {
                    throw new InvalidOperationException("Unable to shut down existing application.");
                }
            }

            // Kick OLE so we can use the clipboard if necessary
            OleInitialize(IntPtr.Zero);

            _application = new Application {
                // Application should survive window closing events to be reusable
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };

            // Dispatcher.Run internally calls PushFrame(new DispatcherFrame()), so we need to call PushFrame ourselves
            _frame = new DispatcherFrame(exitWhenRequested: false);
            List<ExceptionDispatchInfo> exceptionInfos = new List<ExceptionDispatchInfo>();

            // Initialization completed
            ((ManualResetEventSlim)obj).Set();

            while (_frame.Continue) {
                ExceptionDispatchInfo exception = CallSafe(() => Dispatcher.PushFrame(_frame));
                if (exception != null) {
                    exceptionInfos.Add(exception);
                }
            }

            var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            if (dispatcher != null && !dispatcher.HasShutdownStarted) {
                dispatcher.InvokeShutdown();
            }

            if (exceptionInfos.Any()) {
                throw new AggregateException(exceptionInfos.Select(ce => ce.SourceException).ToArray());
            }
        }

        private void Destroy(object sender, EventArgs e) {
            AppDomain.CurrentDomain.DomainUnload -= Destroy;
            AppDomain.CurrentDomain.ProcessExit -= Destroy;

            Thread mainThread = _thread;
            _thread = null;
            _frame.Continue = false;

            // If the thread is still alive, allow it to exit normally so the dispatcher can continue to clear pending work items
            // 10 seconds should be enough
            mainThread.Join(10000);
        }

        private static ExceptionDispatchInfo CallSafe(Action action) {
            return CallSafe<object>(() => {
                action();
                return null;
            }).Exception;
        }

        private static CallSafeResult<T> CallSafe<T>(Func<T> func) {
            try {
                return new CallSafeResult<T> { Value = func() };
            } catch (ThreadAbortException tae) {
                // Thread should be terminated anyway
                Thread.ResetAbort();
                return new CallSafeResult<T> { Exception = ExceptionDispatchInfo.Capture(tae) };
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
