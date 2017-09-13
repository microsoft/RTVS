// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Common.Core.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsMainThread : IMainThread {
        private readonly Dispatcher _mainThreadDispatcher;
        private readonly Thread _mainThread;

        public VsMainThread() {
            _mainThread = ThreadHelper.JoinableTaskContext.MainThread;
            _mainThreadDispatcher = Dispatcher.FromThread(_mainThread);
        }

        #region IMainThread
        public int ThreadId => _mainThread.ManagedThreadId;

        public void Post(Action action, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_mainThreadDispatcher.HasShutdownStarted) {
                return;
            }

            var awaiter = ThreadHelper.JoinableTaskFactory
                .SwitchToMainThreadAsync(cancellationToken)
                .GetAwaiter();

            awaiter.OnCompleted(action);
        }

        public void Send(Action action)
            => ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync().GetAwaiter().OnCompleted(action);

        public async Task SendAsync(Action action, CancellationToken cancellationToken = default(CancellationToken)) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!cancellationToken.IsCancellationRequested) {
                action();
            }
        }
        public async Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken = default(CancellationToken)) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            return !cancellationToken.IsCancellationRequested ? action() : default(T);
        }
        #endregion
    }
}
