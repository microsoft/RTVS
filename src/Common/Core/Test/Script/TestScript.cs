using System;
using System.Windows.Threading;

namespace Microsoft.Common.Core.Test.Script {
    public abstract class TestScript {
        public static void DoEvents(Dispatcher disp = null) {
            DispatcherFrame frame = new DispatcherFrame();
            if(disp == null) {
                disp = Dispatcher.CurrentDispatcher;
            }

            disp.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public static object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
    }
}
