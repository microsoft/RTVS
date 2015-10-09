using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7BoundBreakpoint : IDebugBoundBreakpoint2 {
        int IDebugBoundBreakpoint2.Delete() {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.Enable(int fEnable) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.GetHitCount(out uint pdwHitCount) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.GetState(enum_BP_STATE[] pState) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.SetCondition(BP_CONDITION bpCondition) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.SetHitCount(uint dwHitCount) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount) {
            throw new NotImplementedException();
        }
    }
}