using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7BoundBreakpoint : IDebugBoundBreakpoint2 {
        private enum_BP_STATE _state;
        public AD7PendingBreakpoint PendingBreakpoint { get; }

        public AD7Engine Engine => PendingBreakpoint.Engine;

        public event EventHandler Deleted;

        public AD7BoundBreakpoint(AD7PendingBreakpoint pendingBreakpoint, enum_PENDING_BP_STATE state) {
            PendingBreakpoint = pendingBreakpoint;
            _state = (enum_BP_STATE)state;
        }

        int IDebugBoundBreakpoint2.Delete() {
            _state = enum_BP_STATE.BPS_DELETED;
            Deleted?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.Enable(int fEnable) {
            if (_state == enum_BP_STATE.BPS_DELETED) {
                Debug.Fail($"Trying to enable or disable a deleted {nameof(AD7BoundBreakpoint)}");
                return VSConstants.E_FAIL;
            }

            _state = fEnable == 0 ? enum_BP_STATE.BPS_DISABLED : enum_BP_STATE.BPS_ENABLED;
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution) {
            throw new NotImplementedException();
        }

        int IDebugBoundBreakpoint2.GetHitCount(out uint pdwHitCount) {
            pdwHitCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugBoundBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint) {
            ppPendingBreakpoint = PendingBreakpoint;
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetState(enum_BP_STATE[] pState) {
            pState[0] = _state;
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.SetCondition(BP_CONDITION bpCondition) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugBoundBreakpoint2.SetHitCount(uint dwHitCount) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugBoundBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount) {
            return VSConstants.E_NOTIMPL;
        }
    }
}