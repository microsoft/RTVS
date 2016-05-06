// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7BreakpointResolution : IDebugBreakpointResolution2 {
        public AD7BoundBreakpoint BoundBreakpoint { get; }

        public AD7Engine Engine => BoundBreakpoint.Engine;

        public AD7BreakpointResolution(AD7BoundBreakpoint boundBreakpoint) {
            BoundBreakpoint = boundBreakpoint;
        }

        int IDebugBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType) {
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        int IDebugBreakpointResolution2.GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo) {
            if (dwFields.HasFlag(enum_BPRESI_FIELDS.BPRESI_PROGRAM)) {
                pBPResolutionInfo[0].pProgram = Engine;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
            }

            if (dwFields.HasFlag(enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION)) {
                string fileName;
                int lineNumber;
                TEXT_POSITION start, end;
                BoundBreakpoint.PendingBreakpoint.GetLocation(out fileName, out lineNumber, out start, out end);

                var addr = new AD7MemoryAddress(Engine, fileName, lineNumber);
                addr.DocumentContext = new AD7DocumentContext(fileName, start, end, addr);

                var location = new BP_RESOLUTION_LOCATION {
                    bpType = (uint)enum_BP_TYPE.BPT_CODE,
                    unionmember1 = Marshal.GetComInterfaceForObject(addr, typeof(IDebugCodeContext2))
                };

                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
            }

            return VSConstants.S_OK;
        }
    }
}
