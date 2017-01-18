// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Languages.Editor.Test.Shell {
    public class TestShellBase : IMainThread {
        public Thread MainThread { get; set; }

        public TestShellBase() {
            MainThread = Thread.CurrentThread;
            FileDialog = new TestFileDialog();
            ProgressDialog = new TestProgressDialog();
        }

        public void ShowErrorMessage(string msg) { }

        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            return MessageButtons.OK;
        }

        public void ShowContextMenu(CommandID commandId, int x, int y, object commandTaget = null) { }

        public string SaveFileIfDirty(string fullPath) => fullPath;

        public void UpdateCommandStatus(bool immediate) { }

        public void DoIdle() {
            UIThreadHelper.Instance.Invoke(() => Idle?.Invoke(null, EventArgs.Empty));
            DoEvents();
        }

        private void FireIdle() {
            Idle?.Invoke(null, EventArgs.Empty);
        }

        public void DispatchOnUIThread(Action action) {
            UIThreadHelper.Instance.InvokeAsync(action).DoNotWait(); 
        }

        public void DoEvents() {
            var disp = GetDispatcher();
            if (disp != null) {
                DispatcherFrame frame = new DispatcherFrame();
                disp.BeginInvoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
            }
        }

        public object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        private Dispatcher GetDispatcher(Thread thread = null) {
            if (thread == null) {
                if (MainThread != null && MainThread.ManagedThreadId == UIThreadHelper.Instance.Thread.ManagedThreadId) {
                    return Dispatcher.FromThread(MainThread);
                }
            } else {
                return Dispatcher.FromThread(thread);
            }
            return null;
        }

        public event EventHandler<EventArgs> Idle;
#pragma warning disable 0067
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 0067

        #region IMainThread
        public int ThreadId => MainThread.ManagedThreadId;
        public void Post(Action action, CancellationToken cancellationToken) => UIThreadHelper.Instance.InvokeAsync(action, cancellationToken).DoNotWait();
        #endregion

        #region ICoreShell
        public bool IsUnitTestEnvironment { get; set; } = true;
        public IApplicationConstants AppConstants => new TestAppConstants();
        public virtual ICoreServices Services => TestCoreServices.CreateReal();
        public IProgressDialog ProgressDialog { get; }
        public IFileDialog FileDialog { get; }
        #endregion
    }
}
