using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    [ComVisible(true)]
    [Guid(DebuggerGuids.CustomViewerString)]
    public class AD7CustomViewer : IDebugCustomViewer {
        public AD7CustomViewer() {
        }

        public int DisplayValue(IntPtr hwnd, uint dwID, object pHostServices, IDebugProperty3 pDebugProperty) {
            var property = pDebugProperty as AD7Property;
            if (property == null || property.StackFrame.Engine.GridViewProvider == null) {
                return VSConstants.E_FAIL;
            }

            property.StackFrame.Engine.GridViewProvider.ShowDataGrid(property.EvaluationResult);
            return VSConstants.S_OK;
        }
    }
}
