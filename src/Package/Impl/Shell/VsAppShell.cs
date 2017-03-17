// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell provides access to services
    /// such as composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    [Export(typeof(ICoreShell))]
    [Export(typeof(IMainThread))]
    public sealed partial class VsAppShell : ICoreShell, IIdleTimeSource, IVsShellPropertyEvents, IDisposable {
        private static VsAppShell _instance;
        private static ICoreShell _testShell;

        /// <summary>
        /// Current application shell instance. Provides access to services
        /// such as composition container, export provider, global VS IDE
        /// services and so on.
        /// </summary>
        public static ICoreShell Current {
            get {
                if (_testShell == null && _instance == null) {
                    // Try test environment
                    CoreShell.TryCreateTestInstance("Microsoft.VisualStudio.R.Package.Test.dll", "TestVsshell");
                }

                return _testShell ?? GetInstance();
            }
            internal set {
                // Normally only called in test cases when package
                // is not loaded and hence shell is not initialized.
                // In this case test code provides replacement shell
                // which we then pass to any other shell-type objects
                // to use.
                if (_instance != null) {
                    throw new InvalidOperationException("Cannot set test shell when real one is already there.");
                }
                if (_testShell == null) {
                    _testShell = value;
                }
            }
        }

        #region ICoreShell
        /// <summary>
        /// Provides a way to execute action on UI thread while
        /// UI thread is waiting for the completion of the action.
        /// May be implemented using ThreadHelper in VS or via
        /// SynchronizationContext in all-managed application.
        /// 
        /// This can be blocking or non blocking dispatch, preferrably
        /// non blocking
        /// </summary>
        /// <param name="action">Action to execute</param>
        public void DispatchOnUIThread(Action action) {
            if (MainThread != null) {
                Debug.Assert(MainThreadDispatcher != null);

                if (MainThreadDispatcher != null && !MainThreadDispatcher.HasShutdownStarted) {
                    MainThreadDispatcher.BeginInvoke(action, DispatcherPriority.Normal);
                }
            } else {
                Debug.Assert(false);
                ThreadHelper.Generic.BeginInvoke(DispatcherPriority.Normal, () => action());
            }
        }

        private Dispatcher MainThreadDispatcher { get; set; }

        /// <summary>
        /// Provides access to the application main thread, so users can know if the task they are trying
        /// to execute is executing from the right thread.
        /// </summary>
        public Thread MainThread { get; private set; }

        /// <summary>
        /// Fires when host application has completed it's startup sequence
        /// </summary>
        public event EventHandler<EventArgs> Started;

        /// <summary>
        /// Fires when host application is terminating
        /// </summary>
        public event EventHandler<EventArgs> Terminating;

        public bool IsUnitTestEnvironment { get; set; }
        #endregion

        #region IMainThread
        public int ThreadId => MainThread.ManagedThreadId;
        public void Post(Action action, CancellationToken cancellationToken) {
            if (MainThreadDispatcher.HasShutdownStarted) {
                throw new InvalidOperationException("Unable to transition to UI thread: dispatcher has started shutdown.");
            }

            var awaiter = ThreadHelper.JoinableTaskFactory
                .SwitchToMainThreadAsync(cancellationToken)
                .GetAwaiter();

            awaiter.OnCompleted(action);
        }
        #endregion
    }
}
