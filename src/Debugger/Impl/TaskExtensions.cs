// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Debugger {
    internal static class TaskExtensions {
        private static readonly Lazy<IVsThreadedWaitDialogFactory> _twdf = new Lazy<IVsThreadedWaitDialogFactory>(() => {
            var twdf = (IVsThreadedWaitDialogFactory)Package.GetGlobalService(typeof(SVsThreadedWaitDialogFactory));
            if (twdf == null) {
                var err = Invariant($"{nameof(SVsThreadedWaitDialogFactory)} is not available");
                Trace.Fail(err);
                throw new InvalidOperationException(err);
            }
            return twdf;
        });

        public static void RunSynchronouslyOnUIThread(Func<CancellationToken, Task> method, double delayToShowDialog = 2) {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            using (var session = StartWaitDialog(delayToShowDialog)) {
                var ct = session.UserCancellationToken;
                ThreadHelper.JoinableTaskFactory.Run(() => method(ct));
            }
        }

        public static T RunSynchronouslyOnUIThread<T>(Func<CancellationToken, Task<T>> method, double delayToShowDialog = 2) {
            T result;
            using (var session = StartWaitDialog(delayToShowDialog)) {
                var ct = session.UserCancellationToken;
                result = ThreadHelper.JoinableTaskFactory.Run(() => method(ct));
            }

            return result;
        }

        private static ThreadedWaitDialogHelper.Session StartWaitDialog(double delayToShowDialog) {
            var initialProgress = new ThreadedWaitDialogProgressData(Resources.DebuggerInProgress, isCancelable: true);
            return _twdf.Value.StartWaitDialog(null, initialProgress, TimeSpan.FromSeconds(delayToShowDialog));
        }
    }
}