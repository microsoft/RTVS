// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal class VsProgressDialog : IProgressDialog {
        private readonly IServiceContainer _services;

        public VsProgressDialog(IServiceContainer services) {
            _services = services;
        }

        public void Show(Func<CancellationToken, System.Threading.Tasks.Task> method, string waitMessage, int delayToShowDialogMs = 0) {
            using (var session = StartWaitDialog(waitMessage, delayToShowDialogMs)) {
                var ct = session.UserCancellationToken;
                ThreadHelper.JoinableTaskFactory.Run(() => method(ct));
            }
        }

        public T Show<T>(Func<CancellationToken, System.Threading.Tasks.Task<T>> method, string waitMessage, int delayToShowDialogMs = 0) {
            T result;
            using (var session = StartWaitDialog(waitMessage, delayToShowDialogMs)) {
                var ct = session.UserCancellationToken;
                result = ThreadHelper.JoinableTaskFactory.Run(() => method(ct));
            }
            return result;
        }

        public void Show(Func<IProgress<ProgressDialogData>, CancellationToken, System.Threading.Tasks.Task> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0) {
            using (var session = StartWaitDialog(waitMessage, delayToShowDialogMs)) {
                var ct = session.UserCancellationToken;
                var progress = new Progress(session.Progress, totalSteps); 
                ThreadHelper.JoinableTaskFactory.Run(() => method(progress, ct));
            }
        }

        public T Show<T>(Func<IProgress<ProgressDialogData>, CancellationToken, System.Threading.Tasks.Task<T>> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0) {
            T result;
            using (var session = StartWaitDialog(waitMessage, delayToShowDialogMs)) {
                var ct = session.UserCancellationToken;
                var progress = new Progress(session.Progress, totalSteps);
                result = ThreadHelper.JoinableTaskFactory.Run(() => method(progress, ct));
            }
            return result;
        }

        private ThreadedWaitDialogHelper.Session StartWaitDialog(string waitMessage, int delayToShowDialogMs) {
            var dialogFactory = _services.GetService<IVsThreadedWaitDialogFactory>(typeof(SVsThreadedWaitDialogFactory));
            var initialProgress = new ThreadedWaitDialogProgressData(waitMessage, isCancelable: true);
            return dialogFactory.StartWaitDialog(null, initialProgress, TimeSpan.FromMilliseconds(delayToShowDialogMs));
        }

        private class Progress : IProgress<ProgressDialogData> {
            private readonly IProgress<ThreadedWaitDialogProgressData> _vsProgress;
            private readonly int _totalSteps;

            public Progress(IProgress<ThreadedWaitDialogProgressData> vsProgress, int totalSteps) {
                _vsProgress = vsProgress;
                _totalSteps = totalSteps;
            }

            public void Report(ProgressDialogData data) {
                _vsProgress.Report(new ThreadedWaitDialogProgressData(data.WaitMessage, data.ProgressText, data.StatusBarText, true, data.Step, _totalSteps));
            }
        }
    }
}