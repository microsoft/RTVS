using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Debugger.Engine {
    internal static class TaskExtensions {
        private static readonly Lazy<IVsThreadedWaitDialogFactory> _twdf = new Lazy<IVsThreadedWaitDialogFactory>(() => {
            var twdf = (IVsThreadedWaitDialogFactory)Package.GetGlobalService(typeof(SVsThreadedWaitDialogFactory));
            if (twdf == null) {
                string err = $"{nameof(SVsThreadedWaitDialogFactory)} is not available";
                Trace.Fail(err);
                throw new InvalidOperationException(err);
            }
            return twdf;
        });

        public static void GetResultOnUIThread(this Task task, int delay = 1) {
            var syncContext = SynchronizationContext.Current;
            if (syncContext == null) {
                string err = $"{nameof(GetResultOnUIThread)} called from a background thread";
                Trace.Fail(err);
                throw new InvalidOperationException(err);
            }

            var twdf = _twdf.Value;
            IVsThreadedWaitDialog2 twd;
            Marshal.ThrowExceptionForHR(twdf.CreateInstance(out twd));

            // Post EndWaitDialog rather than invoking directly, so that it is guaranteed to be invoked after StartWaitDialog.
            task.ContinueWith(delegate { syncContext.Post(delegate { twd.EndWaitDialog(); }, null); });
            twd.StartWaitDialog(null, "Debugger operation is in progress...", null, null, null, delay, false, true);

            task.GetAwaiter().GetResult();
        }

        public static TResult GetResultOnUIThread<TResult>(this Task<TResult> task, int delay = 1) {
            GetResultOnUIThread((Task)task, delay);
            return task.GetAwaiter().GetResult();
        }
    }
}