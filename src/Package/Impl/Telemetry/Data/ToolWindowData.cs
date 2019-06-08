// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Data {
    /// <summary>
    /// Data on a tool window as reported in the telemetry
    /// </summary>
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
        public static IEnumerable<ToolWindowData> GetToolWindowData(IVsUIShell shell) {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            var data = new List<ToolWindowData>();
            try {
                IEnumWindowFrames e;
                shell.GetToolWindowEnum(out e);

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

                    data.Add(d);
                }
            } catch (Exception) { }

            return data;
        }
    }
}
