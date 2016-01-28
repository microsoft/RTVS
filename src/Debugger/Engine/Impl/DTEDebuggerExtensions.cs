using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Debugger.Engine {
    public static class DTEDebuggerExtensions {
        /// <summary>
        /// Forces debugger to refresh its variable views (Locals, Autos etc) by re-querying the debug engine.
        /// </summary>
        /// <param name="debugger"></param>
        public static void RefreshVariableViews(this EnvDTE.Debugger debugger) {
            // There's no proper way to do this, so just "change" a property that would invalidate the view.
            debugger.HexDisplayMode = debugger.HexDisplayMode;
        }
    }
}
