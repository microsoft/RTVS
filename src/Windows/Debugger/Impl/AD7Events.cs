// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal class AD7AsynchronousEvent : IDebugEvent2 {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes) {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    internal class AD7StoppingEvent : IDebugEvent2 {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

        int IDebugEvent2.GetAttributes(out uint eventAttributes) {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    internal class AD7SynchronousEvent : IDebugEvent2 {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes) {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7EngineCreateEvent : AD7AsynchronousEvent, IDebugEngineCreateEvent2 {
        public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD06";
        private readonly IDebugEngine2 m_engine;

        AD7EngineCreateEvent(AD7Engine engine) {
            m_engine = engine;
        }

        public static void Send(AD7Engine engine) {
            var eventObject = new AD7EngineCreateEvent(engine);
            engine.Send(eventObject, IID);
        }

        int IDebugEngineCreateEvent2.GetEngine(out IDebugEngine2 engine) {
            engine = m_engine;
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7ProgramCreateEvent : AD7AsynchronousEvent, IDebugProgramCreateEvent2 {
        public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";

        internal static void Send(AD7Engine engine) {
            var eventObject = new AD7ProgramCreateEvent();
            engine.Send(eventObject, IID);
        }
    }

    internal sealed class AD7ExpressionEvaluationCompleteEvent : AD7AsynchronousEvent, IDebugExpressionEvaluationCompleteEvent2 {
        public const string IID = "C0E13A85-238A-4800-8315-D947C960A843";
        private readonly Exception _exception;
        private readonly IDebugExpression2 _expression;
        private readonly IDebugProperty2 _property;

        public AD7ExpressionEvaluationCompleteEvent(IDebugExpression2 expression, IDebugProperty2 property) {
            _expression = expression;
            _property = property;
        }

        public AD7ExpressionEvaluationCompleteEvent(Exception exception) {
            _exception = exception;
        }

        public int GetExpression(out IDebugExpression2 ppExpr) {
            if (_exception != null) {
                throw _exception;
            }

            ppExpr = _expression;
            return VSConstants.S_OK;
        }

        public int GetResult(out IDebugProperty2 ppResult) {
            if (_exception != null) {
                throw _exception;
            }

            ppResult = _property;
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7ModuleLoadEvent : AD7AsynchronousEvent, IDebugModuleLoadEvent2 {
        public const string IID = "989DB083-0D7C-40D1-A9D9-921BF611A4B2";

        readonly AD7Module m_module;
        readonly bool m_fLoad;

        public AD7ModuleLoadEvent(AD7Module module, bool fLoad) {
            m_module = module;
            m_fLoad = fLoad;
        }

        int IDebugModuleLoadEvent2.GetModule(out IDebugModule2 module, ref string debugMessage, ref int fIsLoad) {
            module = m_module;

            if (m_fLoad) {
                debugMessage = null; //String.Concat("Loaded '", m_module.DebuggedModule.Name, "'");
                fIsLoad = 1;
            } else {
                debugMessage = null; // String.Concat("Unloaded '", m_module.DebuggedModule.Name, "'");
                fIsLoad = 0;
            }

            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7ProgramDestroyEvent : AD7SynchronousEvent, IDebugProgramDestroyEvent2 {
        public const string IID = "E147E9E3-6440-4073-A7B7-A65592C714B5";

        readonly uint m_exitCode;
        public AD7ProgramDestroyEvent(uint exitCode) {
            m_exitCode = exitCode;
        }

        int IDebugProgramDestroyEvent2.GetExitCode(out uint exitCode) {
            exitCode = m_exitCode;

            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7ThreadCreateEvent : AD7AsynchronousEvent, IDebugThreadCreateEvent2 {
        public const string IID = "2090CCFC-70C5-491D-A5E8-BAD2DD9EE3EA";
    }

    internal sealed class AD7ThreadDestroyEvent : AD7AsynchronousEvent, IDebugThreadDestroyEvent2 {
        public const string IID = "2C3B7532-A36F-4A6E-9072-49BE649B8541";

        readonly uint m_exitCode;
        public AD7ThreadDestroyEvent(uint exitCode) {
            m_exitCode = exitCode;
        }

        int IDebugThreadDestroyEvent2.GetExitCode(out uint exitCode) {
            exitCode = m_exitCode;

            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7LoadCompleteEvent : AD7StoppingEvent, IDebugLoadCompleteEvent2 {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";

        public AD7LoadCompleteEvent() {
        }
    }

    internal sealed class AD7LoadCompleteRunningEvent : AD7AsynchronousEvent, IDebugLoadCompleteEvent2 {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";

        public AD7LoadCompleteRunningEvent() {
        }
    }

    internal sealed class AD7AsyncBreakCompleteEvent : AD7StoppingEvent, IDebugBreakEvent2 {
        public const string IID = "c7405d1d-e24b-44e0-b707-d8a5a4e1641b";
    }

    internal sealed class AD7SteppingCompleteEvent : AD7StoppingEvent, IDebugStepCompleteEvent2 {
        public const string IID = "0F7F24C1-74D9-4EA6-A3EA-7EDB2D81441D";
    }

    internal sealed class AD7BreakpointBoundEvent : AD7AsynchronousEvent, IDebugBreakpointBoundEvent2 {
        public const string IID = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

        private AD7PendingBreakpoint m_pendingBreakpoint;
        private AD7BoundBreakpoint m_boundBreakpoint;

        public AD7BreakpointBoundEvent(AD7PendingBreakpoint pendingBreakpoint, AD7BoundBreakpoint boundBreakpoint) {
            m_pendingBreakpoint = pendingBreakpoint;
            m_boundBreakpoint = boundBreakpoint;
        }

        int IDebugBreakpointBoundEvent2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum) {
            var boundBreakpoints = new IDebugBoundBreakpoint2[1];
            boundBreakpoints[0] = m_boundBreakpoint;
            ppEnum = new AD7BoundBreakpointEnum(boundBreakpoints);
            return VSConstants.S_OK;
        }

        int IDebugBreakpointBoundEvent2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP) {
            ppPendingBP = m_pendingBreakpoint;
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7BreakpointUnboundEvent : AD7AsynchronousEvent, IDebugBreakpointUnboundEvent2 {
        public const string IID = "78d1db4f-c557-4dc5-a2dd-5369d21b1c8c";

        private AD7BoundBreakpoint m_boundBreakpoint;

        public AD7BreakpointUnboundEvent(AD7BoundBreakpoint boundBreakpoint) {
            m_boundBreakpoint = boundBreakpoint;
        }

        int IDebugBreakpointUnboundEvent2.GetBreakpoint(out IDebugBoundBreakpoint2 ppBP) {
            ppBP = m_boundBreakpoint;
            return VSConstants.S_OK;
        }

        int IDebugBreakpointUnboundEvent2.GetReason(enum_BP_UNBOUND_REASON[] pdwUnboundReason) {
            pdwUnboundReason[0] = enum_BP_UNBOUND_REASON.BPUR_BREAKPOINT_REBIND;
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7BreakpointErrorEvent : AD7AsynchronousEvent, IDebugBreakpointErrorEvent2, IDebugErrorBreakpoint2, IDebugErrorBreakpointResolution2 {
        public const string IID = "abb0ca42-f82b-4622-84e4-6903ae90f210";

        private AD7Engine m_engine;
        private AD7PendingBreakpoint m_pendingBreakpoint;

        public AD7BreakpointErrorEvent(AD7PendingBreakpoint pendingBreakpoint, AD7Engine engine) {
            m_engine = engine;
            m_pendingBreakpoint = pendingBreakpoint;
        }

        int IDebugBreakpointErrorEvent2.GetErrorBreakpoint(out IDebugErrorBreakpoint2 ppErrorBP) {
            ppErrorBP = this;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpoint2.GetBreakpointResolution(out IDebugErrorBreakpointResolution2 ppErrorResolution) {
            ppErrorResolution = this;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint) {
            ppPendingBreakpoint = m_pendingBreakpoint;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType) {
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpointResolution2.GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] pErrorResolutionInfo) {
            pErrorResolutionInfo[0].dwFields = dwFields;
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_PROGRAM) != 0) {
                pErrorResolutionInfo[0].pProgram = (IDebugProgram2)m_engine;
            }
            //if ((dwFields & enum_BPERESI_FIELDS.BPERESI_THREAD) != 0) {
            //    pErrorResolutionInfo[0].pThread = (IDebugThread2)m_engine.MainThread;
            //}
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_TYPE) != 0) {
                pErrorResolutionInfo[0].dwType = enum_BP_ERROR_TYPE.BPET_GENERAL_WARNING;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_BPRESLOCATION) != 0) {
                pErrorResolutionInfo[0].bpResLocation =
                    new BP_RESOLUTION_LOCATION {bpType = (uint) enum_BP_TYPE.BPT_CODE};
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_MESSAGE) != 0) {
                pErrorResolutionInfo[0].bstrMessage = "No code has been loaded for this code location.";
            }
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7EntryPointEvent : AD7StoppingEvent, IDebugEntryPointEvent2 {
        public const string IID = "e8414a3e-1642-48ec-829e-5f4040e16da9";
    }

    internal sealed class AD7BreakpointEvent : AD7StoppingEvent, IDebugBreakpointEvent2 {
        public const string IID = "501C1E21-C557-48B8-BA30-A1EAB0BC4A74";

        IEnumDebugBoundBreakpoints2 m_boundBreakpoints;

        public AD7BreakpointEvent(IEnumDebugBoundBreakpoints2 boundBreakpoints) {
            m_boundBreakpoints = boundBreakpoints;
        }

        int IDebugBreakpointEvent2.EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum) {
            ppEnum = m_boundBreakpoints;
            return VSConstants.S_OK;
        }
    }

    internal sealed class AD7DebugExceptionEvent : AD7StoppingEvent, IDebugExceptionEvent2 {
        public const string IID = "51A94113-8788-4A54-AE15-08B74FF922D0";
        private readonly string _exception, _description;
        private readonly bool _isUnhandled;
        private AD7Engine _engine;

        public AD7DebugExceptionEvent(string typeName, string description, bool isUnhandled, AD7Engine engine) {
            _exception = typeName;
            _description = description;
            _isUnhandled = isUnhandled;
            _engine = engine;
        }

        public int CanPassToDebuggee() {
            return VSConstants.S_FALSE;
        }

        public int GetException(EXCEPTION_INFO[] pExceptionInfo) {
            pExceptionInfo[0].pProgram = _engine;
            pExceptionInfo[0].guidType = DebuggerGuids.DebugEngine;
            pExceptionInfo[0].bstrExceptionName = _exception;
            if (_isUnhandled) {
                pExceptionInfo[0].dwState = enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT;
            } else {
                pExceptionInfo[0].dwState = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE;
            }
            return VSConstants.S_OK;
        }

        public int GetExceptionDescription(out string pbstrDescription) {
            pbstrDescription = _description;
            return VSConstants.S_OK;
        }

        public int PassToDebuggee(int fPass) {
            if (fPass != 0) {
                return VSConstants.S_OK;
            }
            return VSConstants.E_FAIL;
        }
    }

    internal sealed class AD7DebugOutputStringEvent2 : AD7AsynchronousEvent, IDebugOutputStringEvent2 {
        public const string IID = "569C4BB1-7B82-46FC-AE28-4536DDAD753E";
        private readonly string _output;

        public AD7DebugOutputStringEvent2(string output) {
            _output = output;
        }

        public int GetString(out string pbstrString) {
            pbstrString = _output;
            return VSConstants.S_OK;
        }
    }
}
