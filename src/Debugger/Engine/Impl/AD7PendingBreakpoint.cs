using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7PendingBreakpoint : IDebugPendingBreakpoint2 {
        int IDebugPendingBreakpoint2.Bind() {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.Delete() {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.Enable(int fEnable) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount) {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize) {
            throw new NotImplementedException();
        }
    }
}