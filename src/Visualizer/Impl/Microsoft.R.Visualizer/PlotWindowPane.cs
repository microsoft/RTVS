using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.R.Visualizer
{
    [Guid("FE3D0077-CFD8-4178-A755-9B98D0FB6458")]  // Same value as in GuidList.PlotWindowGuidString, TODO: move to better place
    public class PlotWindowPane : ToolWindowPane
    {
        public PlotWindowPane()
        {
            Content = new TextBlock() { Text = "Test string" };
        }
    }
}
