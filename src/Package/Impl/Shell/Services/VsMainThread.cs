// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable VSTHRD004 // Await SwitchToMainThreadAsync

using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Threading;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell
{
    internal sealed class VsMainThread : IMainThread {
        private readonly Dispatcher _mainThreadDispatcher;
        private readonly Thread _mainThread;

        public VsMainThread() {
            _mainThread = ThreadHelper.JoinableTaskContext.MainThread;
            _mainThreadDispatcher = Dispatcher.FromThread(_mainThread);
        }

        #region IMainThread
        public int ThreadId => _mainThread.ManagedThreadId;

        public void Post(Action action) {
            if (_mainThreadDispatcher.HasShutdownStarted) {
                return;
            }

            ThreadHelper.JoinableTaskFactory
                .SwitchToMainThreadAsync()
                .GetAwaiter()
                .OnCompleted(action);
        }

        public IMainThreadAwaiter CreateMainThreadAwaiter(CancellationToken cancellationToken) {
            return new VsMainThreadAwaiter(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken).GetAwaiter());
        }
        #endregion
    }
}
