using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7Thread : IDebugThread2, IDebugThread100, IDisposable {
        private Lazy<IReadOnlyList<DebugStackFrame>> _stackFrames;

        public AD7Engine Engine { get; set; }

        public AD7Thread(AD7Engine engine) {
            Debug.Assert(engine.Session != null);
            Engine = engine;
            Engine.Session.RSession.BeforeRequest += RSession_BeforeRequest;
            ResetStackFrames();
        }

        public void Dispose() {
            Engine.Session.RSession.BeforeRequest -= RSession_BeforeRequest;
            Engine = null;
        }

        private void ThrowIfDisposed() {
            if (Engine == null) {
                throw new ObjectDisposedException(nameof(AD7Thread));
            }
        }

        int IDebugThread100.CanDoFuncEval() {
            ThrowIfDisposed();
            return VSConstants.S_FALSE;
        }

        int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext) {
            ThrowIfDisposed();
            return VSConstants.S_FALSE;
        }

        int IDebugThread2.EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum) {
            ThrowIfDisposed();

            var fi = new FRAMEINFO[1];
            var fis = _stackFrames.Value.Select(f => {
                var frame = (IDebugStackFrame2)new AD7StackFrame(Engine, f);
                Marshal.ThrowExceptionForHR(frame.GetInfo(dwFieldSpec, nRadix, fi));
                return fi[0];
            }).ToArray();
            ppEnum = new AD7FrameInfoEnum(fis);
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread) {
            ppLogicalThread = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread2.GetName(out string pbstrName) {
            ThrowIfDisposed();
            pbstrName = "Main Thread";
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetProgram(out IDebugProgram2 ppProgram) {
            ThrowIfDisposed();
            ppProgram = Engine;
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetThreadId(out uint pdwThreadId) {
            ThrowIfDisposed();
            pdwThreadId = 1;
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp) {
            ThrowIfDisposed();

            var tp = new THREADPROPERTIES();

            if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_ID)) {
                tp.dwThreadId = 1;
                tp.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
            }

            if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_STATE)) {
                tp.dwThreadState = (uint)enum_THREADSTATE.THREADSTATE_RUNNING;
                tp.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
            }

            if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_PRIORITY)) {
                tp.bstrPriority = "Normal";
                tp.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
            }

            if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_NAME)) {
                tp.bstrName = "Main Thread";
                tp.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
            }

            if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_LOCATION)) {
                var frame = _stackFrames.Value.LastOrDefault();
                tp.bstrName = frame != null ? frame.CallingExpression : "<unknown>";
                tp.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
            }

            ptp[0] = tp;
            return VSConstants.S_OK;
        }

        int IDebugThread2.Resume(out uint pdwSuspendCount) {
            pdwSuspendCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread2.SetThreadName(string pszName) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread2.Suspend(out uint pdwSuspendCount) {
            pdwSuspendCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread100.SetFlags(uint flags) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread2.SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread100.GetFlags(out uint pFlags) {
            pFlags = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread100.GetThreadDisplayName(out string bstrDisplayName) {
            bstrDisplayName = null; //""
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread100.GetThreadProperties100(uint dwFields, THREADPROPERTIES100[] ptp) {
            //var ptp7 = new THREADPROPERTIES[1];
            //Marshal.ThrowExceptionForHR(((IDebugThread2)this).GetThreadProperties((enum_THREADPROPERTY_FIELDS)(dwFields & 0x3F), ptp7));

            //var tp = new THREADPROPERTIES100 {
            //    bstrLocation = ptp7[0].bstrLocation,
            //    bstrName = ptp7[0].bstrName,
            //    bstrPriority = ptp7[0].bstrPriority,
            //    dwFields = (uint)ptp7[0].dwFields,
            //    dwSuspendCount = ptp7[0].dwSuspendCount,
            //    dwThreadId = ptp7[0].dwThreadId,
            //    dwThreadState = ptp7[0].dwThreadState
            //};

            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread100.SetThreadDisplayName(string bstrDisplayName) {
            return VSConstants.E_NOTIMPL;
        }

        private void ResetStackFrames() {
            _stackFrames = Lazy.Create(() => Engine.Session.GetStackFrames().GetAwaiter().GetResult());
        }

        private void RSession_BeforeRequest(object sender, RRequestEventArgs e) {
            ResetStackFrames();
        }
    }
}
