using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;
using Microsoft.R.Editor.ContentType;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7ProgramNode : IDebugProgramNode2 {
        private readonly uint _pid;

        public AD7ProgramNode(uint pid) {
            _pid = pid;
        }

        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.DetachDebugger_V7() {
            throw new NotImplementedException();
        }

        int IDebugProgramNode2.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine) {
            pbstrEngine = RContentTypeDefinition.LanguageName;
            pguidEngine = DebuggerGuids.DebugEngine;
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetHostMachineName_V7(out string pbstrHostMachineName) {
            pbstrHostMachineName = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName) {
            pbstrHostName = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId) {
            pHostProcessId[0] = new AD_PROCESS_ID();
            pHostProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            pHostProcessId[0].dwProcessId = _pid;
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetProgramName(out string pbstrProgramName) {
            pbstrProgramName = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}