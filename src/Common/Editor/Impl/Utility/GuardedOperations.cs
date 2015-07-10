using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Microsoft.Languages.Editor.Utility
{
    public static class GuardedOperations
    {
        public static void DispatchInvoke(Action toInvoke, DispatcherPriority priority)
        {
            Action guardedAction = 
                () => {
                    try
                    {
                        toInvoke();
                    }
                    catch
                    {
                        Debug.Assert(false, "Guarded invoke caught exception");
                    }
                };

            Dispatcher.CurrentDispatcher.BeginInvoke(guardedAction, priority);
        }
    }
}
