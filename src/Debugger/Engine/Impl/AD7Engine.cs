using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.R.Debugger.Engine.PortSupplier;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Debugger.Engine {
    [ComVisible(true)]
    [Guid(DebuggerGuids.DebugEngineCLSIDString)]
    public sealed class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugProgram3, IDebugSymbolSettings100 {
        private IDebugEventCallback2 _events;
        private RDebugPortSupplier.DebugProgram _program;
        private Guid _programId;
        private bool _firstContinue = true, _sentContinue = false;

        internal bool IsDisposed { get; private set; }

        internal DebugSession DebugSession { get; private set; }

        internal AD7Thread MainThread { get; private set; }

        internal bool IsInBrowseMode { get; set; }

        [Import]
        private IRSessionProvider RSessionProvider { get; set; }

        [Import]
        private IDebugSessionProvider DebugSessionProvider {get;set;}

        public AD7Engine() {
            var compModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            compModel.DefaultCompositionService.SatisfyImportsOnce(this);
        }

        public void Dispose() {
            IsDisposed = true;

            _events = null;
            _program = null;

            MainThread.Dispose();
            MainThread = null;

            DebugSession = null;
            RSessionProvider = null;
            DebugSessionProvider = null;
        }

        private void ThrowIfDisposed() {
            if (IsDisposed) {
                throw new ObjectDisposedException(nameof(AD7Engine));
            }
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
            ThrowIfDisposed();

            if (rgpPrograms.Length != 1) {
                throw new ArgumentException("Zero or more than one programs", "rgpPrograms");
            }

            _program = rgpPrograms[0] as RDebugPortSupplier.DebugProgram;
            if (_program == null) {
                throw new ArgumentException("rgpPrograms[0] must be an " + nameof(RDebugPortSupplier.DebugProgram), "rgpPrograms");
            }

            Marshal.ThrowExceptionForHR(_program.GetProgramId(out _programId));

            DebugSession = DebugSessionProvider.GetDebugSession(_program.Session);
            DebugSession.Browse += Session_Browse;
            DebugSession.RSession.AfterRequest += RSession_AfterRequest;
            DebugSession.Initialize().GetAwaiter().GetResult();

            MainThread = new AD7Thread(this);
            _events = pCallback;

            AD7EngineCreateEvent.Send(this);
            AD7ProgramCreateEvent.Send(this);
            Send(new AD7LoadCompleteEvent(), AD7LoadCompleteEvent.IID);

            return VSConstants.S_OK;
        }

        int IDebugEngine2.CauseBreak() {
            ThrowIfDisposed();
            Task.Run(async delegate {
                await DebugSession.EvaluateAsync(null, "browser()");
            }).GetAwaiter().GetResult();
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 pEvent) {
            ThrowIfDisposed();

            if (pEvent is AD7ProgramDestroyEvent) {
                Dispose();
            }

            return VSConstants.S_OK;
        }

        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            // TODO
            ppPendingBP = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram) {
            ThrowIfDisposed();
            return DebuggerConstants.E_PROGRAM_DESTROY_PENDING;
        }

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 ppEnum) {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.GetEngineId(out Guid pguidEngine) {
            ThrowIfDisposed();
            pguidEngine = DebuggerGuids.DebugEngine;
            return VSConstants.S_OK;
        }

        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType) {
            // TODO
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException) {
            // TODO
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException) {
            // TODO
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.SetLocale(ushort wLangID) {
            ThrowIfDisposed();
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetMetric(string pszMetric, object varValue) {
            ThrowIfDisposed();
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot) {
            ThrowIfDisposed();
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 pProcess) {
            ThrowIfDisposed();
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
            ThrowIfDisposed();
            return VSConstants.S_OK;
        }

        int IDebugProgram2.CauseBreak() {
            ThrowIfDisposed();
            return ((IDebugEngine2)this).CauseBreak();
        }

        int IDebugProgram2.Detach() {
            ThrowIfDisposed();
            Send(new AD7ProgramDestroyEvent(0), AD7ProgramDestroyEvent.IID);
            return VSConstants.S_OK;
        }

        private int Continue(IDebugThread2 pThread) {
            ThrowIfDisposed();

            if (_firstContinue) {
                _firstContinue = false;
            } else {
                // If _sentContinue is true, then this is a dummy Continue issued to notify the
                // debugger that user has explicitly entered something at the Browse prompt. 
                if (!_sentContinue) {
                    _sentContinue = true;
                    DebugSession.ExecuteBrowserCommandAsync("c").DoNotWait();
                }
            }

            return VSConstants.S_OK;
        }

        int IDebugProgram2.Continue(IDebugThread2 pThread) {
            return Continue(pThread);
        }

        int IDebugProgram2.EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum) {
            ThrowIfDisposed();

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
            // TODO
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.EnumThreads(out IEnumDebugThreads2 ppEnum) {
            ThrowIfDisposed();
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
            ThrowIfDisposed();
            pbstrEngine = "R";
            pguidEngine = DebuggerGuids.DebugEngine;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetName(out string pbstrName) {
            ThrowIfDisposed();
            pbstrName = null;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetProcess(out IDebugProcess2 ppProcess) {
            ppProcess = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgram2.GetProgramId(out Guid pguidProgramId) {
            ThrowIfDisposed();
            pguidProgramId = _programId;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step) {
            ThrowIfDisposed();

            Task step;
            switch (sk) {
                case enum_STEPKIND.STEP_OVER:
                    step = DebugSession.StepOverAsync();
                    break;
                case enum_STEPKIND.STEP_INTO:
                    step = DebugSession.StepIntoAsync();
                    break;
                case enum_STEPKIND.STEP_OUT:
                    step = DebugSession.StepOutAsync();
                    break;
                default:
                    return VSConstants.E_NOTIMPL;
            }

            step.ContinueWith(t => {
                Send(new AD7SteppingCompleteEvent(), AD7SteppingCompleteEvent.IID);
            });

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
            ThrowIfDisposed();
            DebugSession.CancelStep();
            return Continue(pThread);
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
            ThrowIfDisposed();
            return VSConstants.S_OK;
        }

        private void Session_Browse(object sender, EventArgs e) {
            IsInBrowseMode = true;
            _sentContinue = false;
            var bps = new AD7BoundBreakpointEnum(new IDebugBoundBreakpoint2[0]);
            var evt = new AD7BreakpointEvent(bps);
            Send(evt, AD7BreakpointEvent.IID);
        }

        private void RSession_AfterRequest(object sender, RRequestEventArgs e) {
            if (IsInBrowseMode) {
                IsInBrowseMode = false;
                if (!_sentContinue) {
                    // User has explicitly typed something at the Browse prompt, so tell the debugger that
                    // we're moving on by issuing a dummy Continue request to switch it to the running state.
                    _sentContinue = true;

                    IDebugProcess2 process;
                    var ex = Marshal.GetExceptionForHR(_program.GetProcess(out process));
                    Debug.Assert(ex == null);
                    if (process != null) {
                        ex = Marshal.GetExceptionForHR(((IDebugProcess3)process).Execute(MainThread));
                        Debug.Assert(ex == null);
                    }
                }
            }
        }
    }
}
