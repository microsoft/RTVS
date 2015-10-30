using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal static class RPlotWindowHost {

        public static void Init() {
            PlotWindowPane pane = ToolWindowUtilities.FindWindowPane<PlotWindowPane>(0);
            RPlotWindowContainer plotContainer = pane.GetIVsWindowPane() as RPlotWindowContainer;
            Debug.Assert(plotContainer != null);

            Control c = plotContainer as Control;
            Debug.Assert(c != null);
            Debug.Assert(c.Handle != IntPtr.Zero);

            RPlotWindowContainerHandle = c.Handle;
        }

        public static IntPtr RPlotWindowContainerHandle { get; private set; }
    }
}
