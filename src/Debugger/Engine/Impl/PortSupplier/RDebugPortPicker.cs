using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine.PortSupplier {
    [ComVisible(true)]
    [Guid("FB6A6E8D-47C2-4D0E-BB44-609887EF2327")]
    public class RDebugPortPicker : IDebugPortPicker {
        public int DisplayPortPicker(IntPtr hwndParentDialog, out string pbstrPortId) {
            pbstrPortId = null;
            return VSConstants.E_NOTIMPL;
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider pSP) {
            return VSConstants.E_NOTIMPL;
        }
    }
}
