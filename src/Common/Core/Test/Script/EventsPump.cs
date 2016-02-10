using System.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Script {
    public static class EventsPump {
        public static void DoEvents(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    TestScript.DoEvents();
                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}
