﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Idle {
    /// <summary>
    /// Asynchronous task that start on next idle slot
    /// </summary>
    public sealed class IdleTimeAsyncTask : CancellableTask {
        private readonly ICoreShell _shell;
        private Func<object> _taskAction;
        private Action<object> _callbackAction;
        private Action<object> _cancelAction;
        private bool _taskRunning;
        private bool _connectedToIdle;
        private long _disposed;
        private int _delay;
        private DateTime _idleConnectTime;
        private ManualResetEvent _taskDoneEvent;

        public object Tag { get; private set; }

        public IdleTimeAsyncTask(ICoreShell shell) {
            _shell = shell;
        }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        /// <param name="cancelAction">Callback to invoke if task is canceled</param>
        /// <param name="shell"></param>
        public IdleTimeAsyncTask(Func<object> taskAction, Action<object> callbackAction, Action<object> cancelAction, ICoreShell shell)
            : this(shell) {
            Debug.Assert(taskAction != null);

            if (taskAction == null)
                throw new ArgumentNullException(nameof(taskAction));

            _taskAction = taskAction;
            _callbackAction = callbackAction;
            _cancelAction = cancelAction;
        }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        /// <param name="shell"></param>
        public IdleTimeAsyncTask(Func<object> taskAction, Action<object> callbackAction, ICoreShell shell)
            : this(taskAction, callbackAction, null, shell) { }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="shell"></param>
        public IdleTimeAsyncTask(Func<object> taskAction, ICoreShell shell)
            : this(taskAction, null, null, shell) { }

        /// <summary>
        /// Run task on next idle slot
        /// </summary>
        public void DoTaskOnIdle() => DoTaskOnIdle(0);

        /// <summary>
        /// Run task on next idle slot after certain amount of milliseconds
        /// </summary>
        public void DoTaskOnIdle(int msDelay) {
            AssertIsOnMainThread();
            Check.InvalidOperation(() => !IsDisposed && _taskAction != null);
            _delay = msDelay;
            ConnectToIdle();
        }

        /// <summary>
        /// Runs specified task on next idle. Task must not be currently running.
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        /// <param name="cancelAction">Callback to invoke if task is canceled</param>
        public void DoTaskOnIdle(Func<object> taskAction, Action<object> callbackAction, Action<object> cancelAction, object tag = null) {
            AssertIsOnMainThread();
            Check.InvalidOperation(() => !TaskRunning);
            Check.ArgumentNull(nameof(taskAction), taskAction);

            Tag = tag;
            _taskAction = taskAction;
            _callbackAction = callbackAction;
            _cancelAction = cancelAction;

            DoTaskOnIdle();
        }

        public bool TaskRunning => _connectedToIdle || _taskRunning;

        private void DoTaskInternal() {
            Debug.Assert(_taskDoneEvent != null);
            Action finalAction;

            if (!IsDisposed) {
                object result = null;

                try {
                    result = _taskAction();
                } catch (Exception ex) {
                    Debug.Fail(String.Format(CultureInfo.CurrentCulture,
                        "Background task exception: {0}.\nInner exception: {1}\nInner exception callstack: {2}",
                        ex.Message,
                        ex.InnerException != null ? ex.InnerException.Message : "(none)",
                        ex.InnerException != null ? ex.InnerException.StackTrace : "(none)"));

                    result = ex;
                } finally {
                    finalAction = () => UIThreadCompletedCallback(result);
                }
            } else {
                finalAction = () => UIThreadCanceledCallback(null);
            }

            _taskDoneEvent.Set();
            _shell.MainThread().Post(finalAction);
        }

        private void UIThreadCompletedCallback(object result) {
            AssertIsOnMainThread();

            try {
                _callbackAction?.Invoke(result);
            } catch (Exception ex) {
                Debug.Fail(String.Format(CultureInfo.CurrentCulture,
                    "Background task UI thread callback exception {0}. Inner exception: {1}",
                    ex.Message, ex.InnerException != null ? ex.InnerException.Message : "(none)"));
            }

            _taskRunning = false;
            _taskDoneEvent.Dispose();
            _taskDoneEvent = null;
        }

        private void UIThreadCanceledCallback(object result) {
            AssertIsOnMainThread();

            _cancelAction?.Invoke(result);
            _taskRunning = false;
            _taskDoneEvent.Dispose();
            _taskDoneEvent = null;
        }

        private void OnIdle(object sender, EventArgs e) {
            AssertIsOnMainThread();

            // Even though disposing will disconnect from idle, that could
            // happen during idle, so this gets called anyway
            if (!IsDisposed && !_taskRunning) {
                if (_delay == 0 || _idleConnectTime.MillisecondsSinceUtc() > _delay) {
                    _taskRunning = true;
                    _taskDoneEvent = new ManualResetEvent(false);

                    DisconnectFromIdle();
                    Task.Run(new Action(DoTaskInternal));
                }
            }
        }

        private void OnTerminate(object sender, EventArgs e) {
            AssertIsOnMainThread();

            // Don't let the app teminate while the background thread is doing work
            _taskDoneEvent?.WaitOne();
            Dispose();
        }

        private void ConnectToIdle() {
            AssertIsOnMainThread();

            if (!_connectedToIdle && !IsDisposed) {
                _connectedToIdle = true;
                _idleConnectTime = DateTime.UtcNow;

                _shell.Idle += OnIdle;
                _shell.Terminating += OnTerminate;
            }
        }

        private void DisconnectFromIdle() {
            AssertIsOnMainThread();

            if (_connectedToIdle) {
                _connectedToIdle = false;

                _shell.Idle -= OnIdle;
                _shell.Terminating -= OnTerminate;
            }
        }

        [Conditional("DEBUG")]
        private void AssertIsOnMainThread() {
            if (!_shell.IsUnitTestEnvironment) {
                _shell.AssertIsOnMainThread();
            }
        }

        #region IDisposable
        private bool IsDisposed => Interlocked.Read(ref _disposed) != 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
            MessageId = "_taskDoneEvent",
            Justification = "The task event is always disposed after the task runs")]
        protected override void Dispose(bool disposing) {
            AssertIsOnMainThread();
            Interlocked.Exchange(ref _disposed, 1);
            DisconnectFromIdle();
            base.Dispose(disposing);
        }
        #endregion 
    }
}
