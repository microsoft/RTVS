using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Languages.Editor.Test.Shell {
    public class TestShellBase {
        public Thread MainThread { get; set; }
        public int LocaleId => 1033;

        public TestShellBase() {
            MainThread = Thread.CurrentThread;
        }

        public void ShowErrorMessage(string msg) { }

        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            return MessageButtons.OK;
        }

        public void ShowContextMenu(CommandID commandId, int x, int y) { }

        public void DoIdle() {
            Idle?.Invoke(null, EventArgs.Empty);
            DoEvents();
        }

        public void DispatchOnUIThread(Action action) {
            UIThreadHelper.Instance.Invoke(action);
        }

        public async Task DispatchOnMainThreadAsync(Action action, CancellationToken cancellationToken = new CancellationToken()) {
            await UIThreadHelper.Instance.InvokeAsync(action);
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

        private Dispatcher GetDispatcher() {
            if (MainThread != null && MainThread.ManagedThreadId == UIThreadHelper.Instance.Thread.ManagedThreadId) {
                return Dispatcher.FromThread(MainThread);
            }
            return null;
        }

        public event EventHandler<EventArgs> Idle;
#pragma warning disable 0067
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 0067
    }
}
