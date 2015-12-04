using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal static class RPlotWindowHost {
        private static IntPtr _handle = IntPtr.Zero;

        public static IntPtr RPlotWindowContainerHandle {
            get {
                if (_handle == IntPtr.Zero) {
                    PlotWindowPane pane = ToolWindowUtilities.FindWindowPane<PlotWindowPane>(0);
                    if (pane != null) {
                        RPlotWindowContainer plotContainer = pane.GetIVsWindowPane() as RPlotWindowContainer;
                        Debug.Assert(plotContainer != null);

                        Control c = (Control)plotContainer;
                        Debug.Assert(c.Handle != IntPtr.Zero);

                        _handle = c.Handle;
                    }
                }
                return _handle;
            }
        }
    }
}
