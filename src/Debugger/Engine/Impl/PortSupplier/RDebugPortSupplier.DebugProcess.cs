/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine.PortSupplier {
    partial class RDebugPortSupplier {
        internal class DebugProcess : IDebugProcess2, IDebugProcessSecurity2 {
            private readonly DebugPort _port;
            private readonly int _sessionId;

            public IRSession Session { get; }

            public string Name => $"R session {_sessionId}";

            public DebugProcess(DebugPort port, int sessionId, IRSession session) {
                _port = port;
                _sessionId = sessionId;
                Session = session;
            }

            public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach) {
                return VSConstants.E_NOTIMPL;
            }

            public int CanDetach() {
                return VSConstants.E_NOTIMPL;
            }

            public int CauseBreak() {
                return VSConstants.E_NOTIMPL;
            }

            public int Detach() {
                return VSConstants.E_NOTIMPL;
            }

            public int EnumPrograms(out IEnumDebugPrograms2 ppEnum) {
                ppEnum = new AD7ProgramEnum(new IDebugProgram2[] { new DebugProgram(this) });
                return VSConstants.S_OK;
            }

            public int EnumThreads(out IEnumDebugThreads2 ppEnum) {
                ppEnum = null;
                return VSConstants.E_NOTIMPL;
            }

            public int GetAttachedSessionName(out string pbstrSessionName) {
                pbstrSessionName = null;
                return VSConstants.E_NOTIMPL;
            }

            public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo) {
                // The various string fields should match the strings returned by GetName - keep them in sync when making any changes here.
                var pi = new PROCESS_INFO();
                pi.Fields = Fields;
                pi.bstrFileName = Name;
                pi.bstrBaseName = Name;
                pi.bstrTitle = "";
                pi.ProcessId.dwProcessId = (uint)_sessionId;
                pProcessInfo[0] = pi;
                return VSConstants.S_OK;
            }

            public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName) {
                // The return value should match the corresponding string field returned from GetInfo - keep them in sync when making any changes here.
                switch (gnType) {
                    case enum_GETNAME_TYPE.GN_FILENAME:
                        pbstrName = Name;
                        break;
                    case enum_GETNAME_TYPE.GN_BASENAME:
                        pbstrName = Name;
                        break;
                    case enum_GETNAME_TYPE.GN_NAME:
                    case enum_GETNAME_TYPE.GN_TITLE:
                        pbstrName = "";
                        break;
                    default:
                        pbstrName = null;
                        break;
                }
                return VSConstants.S_OK;
            }

            public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId) {
                var pidStruct = new AD_PROCESS_ID();
                pidStruct.dwProcessId = (uint)_sessionId;
                pProcessId[0] = pidStruct;
                return VSConstants.S_OK;
            }

            public int GetPort(out IDebugPort2 ppPort) {
                ppPort = _port;
                return VSConstants.S_OK;
            }

            public int GetProcessId(out Guid pguidProcessId) {
                pguidProcessId = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetServer(out IDebugCoreServer2 ppServer) {
                ppServer = null;
                return VSConstants.E_NOTIMPL;
            }

            public int Terminate() {
                return VSConstants.E_NOTIMPL;
            }

            public int GetUserName(out string pbstrUserName) {
                pbstrUserName = null;
                return VSConstants.E_NOTIMPL;
            }

            public int QueryCanSafelyAttach() {
                return VSConstants.S_OK;
            }
        }
    }
}
