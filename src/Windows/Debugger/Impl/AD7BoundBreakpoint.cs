// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.R.ExecutionTracing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    internal sealed class AD7BoundBreakpoint : IDebugBoundBreakpoint2 {
        private enum_BP_STATE _state;
        public AD7PendingBreakpoint PendingBreakpoint { get; }
        public RSourceLocation Location { get; }
        public IRBreakpoint DebugBreakpoint { get; private set; }

        public AD7Engine Engine => PendingBreakpoint.Engine;

        public AD7BoundBreakpoint(AD7PendingBreakpoint pendingBreakpoint, RSourceLocation location, enum_PENDING_BP_STATE state) {
            PendingBreakpoint = pendingBreakpoint;
            Location = location;
            SetState((enum_BP_STATE)state);
        }

        int IDebugBoundBreakpoint2.Delete() {
            return SetState(enum_BP_STATE.BPS_DELETED);
        }

        int IDebugBoundBreakpoint2.Enable(int fEnable) {
            if (_state == enum_BP_STATE.BPS_DELETED) {
                Debug.Fail(Invariant($"Trying to enable or disable a deleted {nameof(AD7BoundBreakpoint)}"));
                return VSConstants.E_FAIL;
            }

            return SetState(_state = fEnable == 0 ? enum_BP_STATE.BPS_DISABLED : enum_BP_STATE.BPS_ENABLED);
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

        private int SetState(enum_BP_STATE state) {
            if (_state == enum_BP_STATE.BPS_ENABLED) {
                if (state == enum_BP_STATE.BPS_DISABLED || state == enum_BP_STATE.BPS_DELETED) {
                    if (DebugBreakpoint != null) {
                        DebugBreakpoint.BreakpointHit -= DebugBreakpoint_BreakpointHit;
                        if (Engine.IsConnected) {
                            if (Engine.IsProgramDestroyed) {
                                // If engine is shutting down, do not wait for the delete eval to complete, to avoid
                                // blocking debugger detach if a long-running operation is in progress. This way the
                                // engine can just report successful detach right away, and breakpoints are deleted
                                // later, but as soon as it's actually possible.
                                DebugBreakpoint.DeleteAsync().DoNotWait();
                            } else {
                                TaskExtensions.RunSynchronouslyOnUIThread(ct => DebugBreakpoint.DeleteAsync(ct));
                            }
                        }
                    }
                }
            } else {
                if (state == enum_BP_STATE.BPS_ENABLED) {
                    if (Engine.IsProgramDestroyed) {
                        // Do not allow enabling breakpoints when engine is shutting down.
                        return VSConstants.E_ABORT;
                    }

                    DebugBreakpoint = TaskExtensions.RunSynchronouslyOnUIThread(ct => Engine.Tracer.CreateBreakpointAsync(Location, ct));
                    DebugBreakpoint.BreakpointHit += DebugBreakpoint_BreakpointHit;
                }
            }

            _state = state;
            return VSConstants.S_OK;
        }

        private void DebugBreakpoint_BreakpointHit(object sender, EventArgs e) {
            var bps = new AD7BoundBreakpointEnum(new IDebugBoundBreakpoint2[] { this });
            var evt = new AD7BreakpointEvent(bps);
            PendingBreakpoint.Engine.Send(evt, AD7BreakpointEvent.IID);
        }
    }
}