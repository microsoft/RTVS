using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Data {
    internal class ToolWindowData {
        public string Caption { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    
        /// <summary>
        /// Retrieves captions and positions of all active tool windows
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ToolWindowData> GetToolWindowData() {
            var data = new List<ToolWindowData>();
            try {
                IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

                IEnumWindowFrames e;
                shell.GetToolWindowEnum(out e);
                e.Reset();

                IVsWindowFrame[] frame = new IVsWindowFrame[1];
                uint fetched = 0;
                while (VSConstants.S_OK == e.Next(1, frame, out fetched) && fetched > 0) {
                    object objCaption;
                    frame[0].GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out objCaption);

                    VSSETFRAMEPOS[] pos = new VSSETFRAMEPOS[1];
                    Guid relative;
                    int x, y, cx, cy;
                    frame[0].GetFramePos(pos, out relative, out x, out y, out cx, out cy);

                    var d = new ToolWindowData() {
                        Caption = objCaption as string,
                        X = x,
                        Y = y,
                        Width = cx,
                        Height = cy
                    };
                }
            } catch (Exception) { }

            return data;
        }
    }
}
