/*
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    [ComVisible(true)]
    [Guid(DebuggerGuids.ProgramProviderCLSIDString)]
    public class AD7ProgramProvider : IDebugProgramProvider2 {
        int IDebugProgramProvider2.GetProviderProcessData(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA[] pProcess) {
            pProcess[0] = new PROVIDER_PROCESS_DATA();

            if (!Flags.HasFlag(enum_PROVIDER_FLAGS.PFLAG_GET_PROGRAM_NODES)) {
                return VSConstants.S_FALSE;
            }

            if (pPort != null && pPort.QueryIsLocal() == VSConstants.S_FALSE) {
                return VSConstants.S_FALSE;
            }

            var process = Process.GetProcessById((int)ProcessId.dwProcessId);
            if (process.ProcessName != "Microsoft.R.Host") {
                return VSConstants.S_FALSE;
            }

            var node = new AD7ProgramNode(ProcessId.dwProcessId);
            IntPtr[] programNodes = { Marshal.GetComInterfaceForObject(node, typeof(IDebugProgramNode2)) };
            var destinationArray = Marshal.AllocCoTaskMem(IntPtr.Size * programNodes.Length);
            Marshal.Copy(programNodes, 0, destinationArray, programNodes.Length);
            pProcess[0].Fields = enum_PROVIDER_FIELDS.PFIELD_PROGRAM_NODES;
            pProcess[0].ProgramNodes.Members = destinationArray;
            pProcess[0].ProgramNodes.dwCount = (uint)programNodes.Length;
            return VSConstants.S_OK;
        }

        int IDebugProgramProvider2.GetProviderProgramNode(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode) {
            ppProgramNode = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramProvider2.SetLocale(ushort wLangID) {
            return VSConstants.S_OK;
        }

        int IDebugProgramProvider2.WatchForProviderEvents(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback) {
            return VSConstants.S_OK;
        }
    }
}
*/