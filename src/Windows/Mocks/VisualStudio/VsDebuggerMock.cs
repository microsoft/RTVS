// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsDebuggerMock : IVsDebugger {
        private Dictionary<uint, IVsDebuggerEvents> _sinks = new Dictionary<uint, IVsDebuggerEvents>();
        private uint _cookie = 1;

        public DBGMODE Mode { get; set; } = DBGMODE.DBGMODE_Design;

        public int AdviseDebugEventCallback(object punkDebuggerEvents) {
            throw new NotImplementedException();
        }

        public int AdviseDebuggerEvents(IVsDebuggerEvents pSink, out uint pdwCookie) {
            pdwCookie = _cookie++;
            _sinks[pdwCookie] = pSink;
            return VSConstants.S_OK;
        }

        public int AllowEditsWhileDebugging(ref Guid guidLanguageService) {
            return VSConstants.S_OK;
        }

        public int ExecCmdForTextPos(VsTextPos[] pTextPos, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return VSConstants.S_OK;
        }

        public int GetDataTipValue(IVsTextLines pTextBuf, TextSpan[] pTS, string pszExpression, out string pbstrValue) {
            pbstrValue = string.Empty;
            return VSConstants.S_OK;
        }

        public int GetENCUpdate(out object ppUpdate) {
            throw new NotImplementedException();
        }

        public int GetMode(DBGMODE[] pdbgmode) {
            pdbgmode[0] = Mode;
            return VSConstants.S_OK;
        }

        public int InsertBreakpointByName(ref Guid guidLanguage, string pszCodeLocationText) {
            throw new NotImplementedException();
        }

        public int IsBreakpointOnName(ref Guid guidLanguage, string pszCodeLocationText, out int pfIsBreakpoint) {
            throw new NotImplementedException();
        }

        public int LaunchDebugTargets(uint cTargets, IntPtr rgDebugTargetInfo) {
            throw new NotImplementedException();
        }

        public int ParseFileRedirection(string pszArgs, out string pbstrArgsProcessed, out IntPtr phStdInput, out IntPtr phStdOutput, out IntPtr phStdError) {
            throw new NotImplementedException();
        }

        public int QueryStatusForTextPos(VsTextPos[] pTextPos, ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            throw new NotImplementedException();
        }

        public int RemoveBreakpointsByName(ref Guid guidLanguage, string pszCodeLocationText) {
            throw new NotImplementedException();
        }

        public int ToggleBreakpointByName(ref Guid guidLanguage, string pszCodeLocationText) {
            throw new NotImplementedException();
        }

        public int UnadviseDebugEventCallback(object punkDebuggerEvents) {
            throw new NotImplementedException();
        }

        public int UnadviseDebuggerEvents(uint dwCookie) {
            _sinks.Remove(dwCookie);
            return VSConstants.S_OK;
        }
    }
}
