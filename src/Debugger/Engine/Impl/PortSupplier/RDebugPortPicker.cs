using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine.PortSupplier {
    [ComVisible(true)]
    [Guid("B8164EAC-B742-4AF3-A61E-49101E4ED117")]
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
