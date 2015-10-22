using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using static System.FormattableString;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7BoundBreakpoint : IDebugBoundBreakpoint2 {
        private enum_BP_STATE _state;
        public AD7PendingBreakpoint PendingBreakpoint { get; }
        public DebugBreakpointLocation Location { get; }
        public DebugBreakpoint DebugBreakpoint { get; private set; }

        public AD7Engine Engine => PendingBreakpoint.Engine;

        public AD7BoundBreakpoint(AD7PendingBreakpoint pendingBreakpoint, DebugBreakpointLocation location, enum_PENDING_BP_STATE state) {
            PendingBreakpoint = pendingBreakpoint;
            Location = location;
            SetState((enum_BP_STATE)state);
        }

        int IDebugBoundBreakpoint2.Delete() {
            SetState(enum_BP_STATE.BPS_DELETED);
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.Enable(int fEnable) {
            if (_state == enum_BP_STATE.BPS_DELETED) {
                Debug.Fail(Invariant($"Trying to enable or disable a deleted {nameof(AD7BoundBreakpoint)}"));
                return VSConstants.E_FAIL;
            }

            SetState(_state = fEnable == 0 ? enum_BP_STATE.BPS_DISABLED : enum_BP_STATE.BPS_ENABLED);
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution) {
            ppBPResolution = null;
            return VSConstants.E_NOTIMPL;
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

        private void SetState(enum_BP_STATE state) {
            if (_state == enum_BP_STATE.BPS_ENABLED) {
                if (state == enum_BP_STATE.BPS_DISABLED || state == enum_BP_STATE.BPS_DELETED) {
                    if (DebugBreakpoint != null) {
                        DebugBreakpoint.BreakpointHit -= DebugBreakpoint_BreakpointHit;
                        DebugBreakpoint.DeleteAsync().GetResultOnUIThread();
                    }
                }
            } else {
                if (state == enum_BP_STATE.BPS_ENABLED) {
                    DebugBreakpoint = Engine.DebugSession.CreateBreakpointAsync(Location).GetResultOnUIThread();
                    DebugBreakpoint.BreakpointHit += DebugBreakpoint_BreakpointHit;
                }
            }

            _state = state;
        }

        private void DebugBreakpoint_BreakpointHit(object sender, EventArgs e) {
            var bps = new AD7BoundBreakpointEnum(new IDebugBoundBreakpoint2[] { this });
            var evt = new AD7BreakpointEvent(bps);
            PendingBreakpoint.Engine.Send(evt, AD7BreakpointEvent.IID);
        }
    }
}