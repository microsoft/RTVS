// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Languages.Editor.Test.Shell {
    public class TestShellBase : ICoreShell {
        protected TestServiceManager ServiceManager { get; }
        protected Thread CreatorThread { get; }

        public TestShellBase(ExportProvider exportProvider) {
            ServiceManager = new TestServiceManager(exportProvider);
            CreatorThread = UIThreadHelper.Instance.Thread;

            ServiceManager.AddService(new TestUIServices());
        }

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
                if (CreatorThread != null && CreatorThread.ManagedThreadId == UIThreadHelper.Instance.Thread.ManagedThreadId) {
                    return Dispatcher.FromThread(CreatorThread);
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
        public int ThreadId => CreatorThread.ManagedThreadId;
        public void Post(Action action, CancellationToken cancellationToken) => UIThreadHelper.Instance.InvokeAsync(action, cancellationToken).DoNotWait();
        #endregion

        #region ICoreShell
        public string ApplicationName => "RTVS_Test";
        public int LocaleId => 1033;
        public bool IsUnitTestEnvironment => true;
        public IServiceContainer Services => ServiceManager;
        #endregion
    }
}
