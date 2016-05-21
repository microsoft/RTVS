// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7ErrorBreakpoint : IDebugErrorBreakpoint2 {
        public AD7PendingBreakpoint PendingBreakpoint { get; }

        public AD7ErrorBreakpointResolution ErrorResolution { get; }
        
        public AD7ErrorBreakpoint(AD7PendingBreakpoint pendingBreakpoint, AD7ErrorBreakpointResolution errorResolution) {
            PendingBreakpoint = pendingBreakpoint;
            ErrorResolution = errorResolution;
        }

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint) {
            ppPendingBreakpoint = PendingBreakpoint;
            return VSConstants.S_OK;
        }

        public int GetBreakpointResolution(out IDebugErrorBreakpointResolution2 ppErrorResolution) {
            ppErrorResolution = ErrorResolution;
            return VSConstants.S_OK;
        }
    }
}