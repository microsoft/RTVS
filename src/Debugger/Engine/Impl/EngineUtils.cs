using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal static class EngineUtils {
        public static void CheckOk(int hr) {
            if (hr != 0) {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public static void RequireOk(int hr) {
            if (hr != 0) {
                throw new InvalidOperationException();
            }
        }

        public static int GetProcessId(this IDebugProcess2 process) {
            AD_PROCESS_ID[] pid = new AD_PROCESS_ID[1];
            EngineUtils.RequireOk(process.GetPhysicalProcessId(pid));

            if (pid[0].ProcessIdType != (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM) {
                return 0;
            }

            return (int)pid[0].dwProcessId;
        }

        public static int GetProcessId(this IDebugProgram2 program) {
            IDebugProcess2 process;
            RequireOk(program.GetProcess(out process));

            return GetProcessId(process);
        }
    }
}
