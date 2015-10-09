using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;
using System;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7BreakpointResolution : IDebugBreakpointResolution2 {
        int IDebugBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType) {
            throw new NotImplementedException();
        }

        int IDebugBreakpointResolution2.GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo) {
            throw new NotImplementedException();
        }
    }
}
