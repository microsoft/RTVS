// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.PortSupplier {
    partial class RDebugPortSupplier {
        internal class DebugProgram : IDebugProgram2 {

            private readonly DebugProcess _process;
            private readonly Guid _guid = Guid.NewGuid();

            public IRSession Session => _process.Session;

            public DebugProgram(DebugProcess process) {
                _process = process;
            }

            public int Attach(IDebugEventCallback2 pCallback) {
                return VSConstants.E_NOTIMPL;
            }

            public int CanDetach() {
                return VSConstants.E_NOTIMPL;
            }

            public int CauseBreak() {
                return VSConstants.E_NOTIMPL;
            }

            public int Continue(IDebugThread2 pThread) {
                return VSConstants.E_NOTIMPL;
            }

            public int Detach() {
                return VSConstants.E_NOTIMPL;
            }

            public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum) {
                ppEnum = null;
                return VSConstants.E_NOTIMPL;
            }

            public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety) {
                ppEnum = null;
                ppSafety = null;
                return VSConstants.E_NOTIMPL;
            }

            public int EnumModules(out IEnumDebugModules2 ppEnum) {
                ppEnum = null;
                return VSConstants.E_NOTIMPL;
            }

            public int EnumThreads(out IEnumDebugThreads2 ppEnum) {
                ppEnum = null;
                return VSConstants.E_NOTIMPL;
            }

            public int Execute() {
                return VSConstants.E_NOTIMPL;
            }

            public int GetDebugProperty(out IDebugProperty2 ppProperty) {
                ppProperty = null;
                return VSConstants.E_NOTIMPL;
            }

            public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream) {
                ppDisassemblyStream = null;
                return VSConstants.E_NOTIMPL;
            }

            public int GetENCUpdate(out object ppUpdate) {
                ppUpdate = null;
                return VSConstants.E_NOTIMPL;
            }

            public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine) {
                pguidEngine = DebuggerGuids.DebugEngine;
                pbstrEngine = null;
                return VSConstants.S_OK;
            }

            public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
                ppMemoryBytes = null;
                return VSConstants.E_NOTIMPL;
            }

            public int GetName(out string pbstrName) {
                pbstrName = null;
                return VSConstants.S_OK;
            }

            public int GetProcess(out IDebugProcess2 ppProcess) {
                ppProcess = _process;
                return VSConstants.S_OK;
            }

            public int GetProgramId(out Guid pguidProgramId) {
                pguidProgramId = _guid;
                return VSConstants.S_OK;
            }

            public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step) {
                return VSConstants.E_NOTIMPL;
            }

            public int Terminate() {
                return VSConstants.E_NOTIMPL;
            }

            public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl) {
                return VSConstants.E_NOTIMPL;
            }
        }
    }
}
