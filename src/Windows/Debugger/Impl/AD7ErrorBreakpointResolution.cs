// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7ErrorBreakpointResolution : IDebugErrorBreakpointResolution2 {
        public string Message { get; }

        public AD7ErrorBreakpointResolution(string message) {
            Message = message;
        }

        public int GetBreakpointType(enum_BP_TYPE[] pBPType) {
            pBPType[0] = enum_BP_TYPE.BPT_NONE;
            return VSConstants.S_OK;
        }

        public int GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] pErrorResolutionInfo) {
            var result = new BP_ERROR_RESOLUTION_INFO {
                dwFields = enum_BPERESI_FIELDS.BPERESI_MESSAGE | enum_BPERESI_FIELDS.BPERESI_TYPE,
                dwType = enum_BP_ERROR_TYPE.BPET_GENERAL_ERROR,
                bstrMessage = Message
            };

            pErrorResolutionInfo[0] = result;
            return VSConstants.S_OK;
        }
    }
}