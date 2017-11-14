// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.PortSupplier {
    partial class RDebugPortSupplier {
        // VS debugger really doesn't like process ID 0, but sessions can have such an ID.
        // So for the debugger, provide fake IDs that are incremented by this amount so that we never get zero.
        internal const uint BaseProcessId = 1000000000;

        public static uint GetProcessId(int sessionId) {
            if (sessionId < 0 || sessionId > 1000000000) {
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            }
            return (uint)sessionId + BaseProcessId;
        }

        internal class DebugProcess : IDebugProcess2, IDebugProcessSecurity2 {
            private readonly DebugPort _port;
            private readonly int _sessionId;

            public IRSession Session { get; }

            public uint ProcessId => RDebugPortSupplier.GetProcessId(_sessionId);

            public string Name => Resources.RSessionNameFormat.FormatInvariant(_sessionId);

            public DebugProcess(DebugPort port, IRSession session) {
                _port = port;
                _sessionId = session.Id;
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
                var pi = new PROCESS_INFO {
                    Fields = Fields,
                    bstrFileName = Name,
                    bstrBaseName = Name,
                    bstrTitle = "",
                    ProcessId = {dwProcessId = ProcessId}
                };
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
                var pidStruct = new AD_PROCESS_ID {dwProcessId = ProcessId};
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
