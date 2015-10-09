using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.R.Host.Client;
using Microsoft.Common.Core;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Debugger.Engine {
    [ComVisible(true)]
    [Guid(DebuggerGuids.DebugEngineCLSIDString)]
    public sealed class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugProgram3, IDebugSymbolSettings100 {
        private bool _firstContinue = true;

        internal DebugSession Session { get; private set; }

        internal AD7Thread MainThread { get; }

        [Import]
        private IRSessionProvider SessionProvider { get; set; }

        private IDebugEventCallback2 _events;
        private Guid _programId;

        public AD7Engine() {
            var compModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            compModel.DefaultCompositionService.SatisfyImportsOnce(this);
            MainThread = new AD7Thread(this);
        }

        internal void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread) {
            var events = _events;
            if (events == null) {
                return;
            }

            uint attributes;
            var riidEvent = new Guid(iidEvent);
            Marshal.ThrowExceptionForHR(eventObject.GetAttributes(out attributes));

            if ((attributes & (uint)enum_EVENTATTRIBUTES.EVENT_STOPPING) != 0 && thread == null) {
                throw new InvalidOperationException("A thread must be provided for a stopping event");
            }

            try {
                Marshal.ThrowExceptionForHR(events.Event(this, null, program, thread, eventObject, ref riidEvent, attributes));
            } catch (InvalidCastException) {
                // COM object has gone away.
            }
        }

        internal void Send(IDebugEvent2 eventObject, string iidEvent) {
            Send(eventObject, iidEvent, this, MainThread);
        }

        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason) {
            if (rgpPrograms.Length != 1) {
                throw new ArgumentException("Zero or more than one programs", "rgpPrograms");
            }
            if (rgpProgramNodes.Length != 1 || !(rgpProgramNodes[0] is AD7ProgramNode)) {
                throw new ArgumentException("Zero or more than one program node, or program node is not a " + typeof(AD7ProgramNode), "rgpProgramNodes");
            }

            Marshal.ThrowExceptionForHR(rgpPrograms[0].GetProgramId(out _programId));

            var session = SessionProvider.Current;
            if (session == null) {
                throw new InvalidOperationException("No session");
            }

            Session = new DebugSession(session);
            Session.BreakpointHit += Session_Paused;
            Session.Resumed += Session_Resumed;

            Task.Run(async delegate {
                await Session.Initialize();
            }).GetAwaiter().GetResult();

            _events = pCallback;
            AD7EngineCreateEvent.Send(this);
            AD7ProgramCreateEvent.Send(this);
            Send(new AD7LoadCompleteEvent(), AD7LoadCompleteEvent.IID);

            return VSConstants.S_OK;
        }

        int IDebugEngine2.CauseBreak() {
            Task.Run(async delegate {
                await Session.EvaluateAsync(null, "browser()");
            }).GetAwaiter().GetResult();
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 pEvent) {
            if (pEvent is AD7ProgramDestroyEvent) {
                _events = null;
            }

            return VSConstants.S_OK;
        }

        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            throw new NotImplementedException();
        }

        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram) {
            return DebuggerConstants.E_PROGRAM_DESTROY_PENDING;
        }

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 ppEnum) {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.GetEngineId(out Guid pguidEngine) {
            pguidEngine = DebuggerGuids.DebugEngine;
            return VSConstants.S_OK; ;
        }

        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType) {
            throw new NotImplementedException();
        }

        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException) {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException) {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetLocale(ushort wLangID) {
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetMetric(string pszMetric, object varValue) {
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot) {
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 pProcess) {
            return VSConstants.S_FALSE;
        }

        int IDebugEngineLaunch2.LaunchSuspended(string pszServer, IDebugPort2 pPort, string pszExe, string pszArgs, string pszDir, string bstrEnv, string pszOptions, enum_LAUNCH_FLAGS dwLaunchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 pCallback, out IDebugProcess2 ppProcess) {
            throw new NotImplementedException();
        }

        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 pProcess) {
            throw new NotImplementedException();
        }

        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 pProcess) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.Attach(IDebugEventCallback2 pCallback) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.CanDetach() {
            return VSConstants.S_OK;
        }

        int IDebugProgram2.CauseBreak() {
            return ((IDebugEngine2)this).CauseBreak();
        }

        int IDebugProgram2.Detach() {
            Send(new AD7ProgramDestroyEvent(0), AD7ProgramDestroyEvent.IID);
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Continue(IDebugThread2 pThread) {
            if (_firstContinue) {
                _firstContinue = false;
            } else {
                Session.ExecuteAsync("c").DoNotWait();
            }
            return VSConstants.S_OK;
        }

        int IDebugProgram2.EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum) {
            string fileName;
            Marshal.ThrowExceptionForHR(pDocPos.GetFileName(out fileName));

            var start = new TEXT_POSITION[1];
            var end = new TEXT_POSITION[1];
            Marshal.ThrowExceptionForHR(pDocPos.GetRange(start, end));

            var addr = new AD7MemoryAddress(this, fileName, (int)start[0].dwLine);
            ppEnum = new AD7CodeContextEnum(new[] { addr });
            return VSConstants.S_OK;
        }

        int IDebugProgram2.EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety) {
            ppEnum = null;
            ppSafety = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.EnumModules(out IEnumDebugModules2 ppEnum) {
            throw new NotImplementedException();
        }

        int IDebugProgram2.EnumThreads(out IEnumDebugThreads2 ppEnum) {
            ppEnum = new AD7ThreadEnum(new[] { MainThread });
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Execute() {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetDebugProperty(out IDebugProperty2 ppProperty) {
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream) {
            ppDisassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetENCUpdate(out object ppUpdate) {
            ppUpdate = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine) {
            pbstrEngine = "R";
            pguidEngine = DebuggerGuids.DebugEngine;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetName(out string pbstrName) {
            pbstrName = null;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetProcess(out IDebugProcess2 ppProcess) {
            ppProcess = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetProgramId(out Guid pguidProgramId) {
            pguidProgramId = _programId;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step) {
            string[] cmds;
            switch (sk) {
                case enum_STEPKIND.STEP_OVER:
                    cmds = new[] { "n" };
                    break;
                case enum_STEPKIND.STEP_INTO:
                    cmds = new[] { "s" };
                    break;
                case enum_STEPKIND.STEP_OUT:
                    cmds = new[] { "browserSetDebug()", "c" };
                    break;
                default:
                    return VSConstants.E_NOTIMPL;
            }

            foreach (var cmd in cmds) {
                Session.ExecuteAsync(cmd).DoNotWait();
            }

            return VSConstants.S_OK;
        }

        int IDebugProgram2.Terminate() {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram3.Attach(IDebugEventCallback2 pCallback) {
            return ((IDebugProgram2)this).Attach(pCallback);
        }

        int IDebugProgram3.CanDetach() {
            return ((IDebugProgram2)this).CanDetach();
        }

        int IDebugProgram3.CauseBreak() {
            return ((IDebugProgram2)this).CauseBreak();
        }

        int IDebugProgram3.Continue(IDebugThread2 pThread) {
            return ((IDebugProgram2)this).Continue(pThread);
        }

        int IDebugProgram3.Detach() {
            return ((IDebugProgram2)this).Detach();
        }

        int IDebugProgram3.EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum) {
            return ((IDebugProgram2)this).EnumCodeContexts(pDocPos, out ppEnum);
        }

        int IDebugProgram3.EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety) {
            return ((IDebugProgram2)this).EnumCodePaths(pszHint, pStart, pFrame, fSource, out ppEnum, out ppSafety);
        }

        int IDebugProgram3.EnumModules(out IEnumDebugModules2 ppEnum) {
            return ((IDebugProgram2)this).EnumModules(out ppEnum);
        }

        int IDebugProgram3.EnumThreads(out IEnumDebugThreads2 ppEnum) {
            return ((IDebugProgram2)this).EnumThreads(out ppEnum);
        }

        int IDebugProgram3.Execute() {
            return ((IDebugProgram2)this).Execute();
        }

        int IDebugProgram3.ExecuteOnThread(IDebugThread2 pThread) {
            return ((IDebugProgram2)this).Continue(pThread);
        }

        int IDebugProgram3.GetDebugProperty(out IDebugProperty2 ppProperty) {
            return ((IDebugProgram2)this).GetDebugProperty(out ppProperty);
        }

        int IDebugProgram3.GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream) {
            return ((IDebugProgram2)this).GetDisassemblyStream(dwScope, pCodeContext, out ppDisassemblyStream);
        }

        int IDebugProgram3.GetENCUpdate(out object ppUpdate) {
            return ((IDebugProgram2)this).GetENCUpdate(out ppUpdate);
        }

        int IDebugProgram3.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine) {
            return ((IDebugProgram2)this).GetEngineInfo(out pbstrEngine, out pguidEngine);
        }

        int IDebugProgram3.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            return ((IDebugProgram2)this).GetMemoryBytes(out ppMemoryBytes);
        }

        int IDebugProgram3.GetName(out string pbstrName) {
            return ((IDebugProgram2)this).GetName(out pbstrName);
        }

        int IDebugProgram3.GetProcess(out IDebugProcess2 ppProcess) {
            return ((IDebugProgram2)this).GetProcess(out ppProcess);
        }

        int IDebugProgram3.GetProgramId(out Guid pguidProgramId) {
            throw new NotImplementedException();
        }

        int IDebugProgram3.Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step) {
            return ((IDebugProgram2)this).Step(pThread, sk, Step);
        }

        int IDebugProgram3.Terminate() {
            return ((IDebugProgram2)this).Terminate();
        }

        int IDebugProgram3.WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl) {
            return ((IDebugProgram2)this).WriteDump(DUMPTYPE, pszDumpUrl);
        }

        int IDebugSymbolSettings100.SetSymbolLoadState(int bIsManual, int bLoadAdjacentSymbols, string bstrIncludeList, string bstrExcludeList) {
            return VSConstants.S_OK;
        }

        private void Session_Paused(object sender, EventArgs e) {
            var bps = new AD7BoundBreakpointsEnum(new IDebugBoundBreakpoint2[0]);
            var evt = new AD7BreakpointEvent(bps);
            Send(evt, AD7BreakpointEvent.IID);
        }

        private void Session_Resumed(object sender, EventArgs e) {
        }
    }
}
