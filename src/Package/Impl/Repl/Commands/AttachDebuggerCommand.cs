using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Debugger.Engine;
using Microsoft.R.Debugger.Engine.PortSupplier;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class AttachDebuggerCommand : ViewCommand {
        private static readonly CommandId[] _commands =  {
            new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdAttachDebugger)
        };

        private ReplWindow _replWindow;

        public AttachDebuggerCommand(ITextView textView)
            : base(textView, _commands, false) {
            ReplWindow.EnsureReplWindow().DoNotWait();
            _replWindow = ReplWindow.Current;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public unsafe override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var evaluator = _replWindow.GetInteractiveWindow().InteractiveWindow.Evaluator as RInteractiveEvaluator;
            if (evaluator == null) {
                return CommandResult.Disabled;
            }

            var debugger = AppShell.Current.GetGlobalService<IVsDebugger2>(typeof(SVsShellDebugger));
            if (debugger == null) {
                return CommandResult.Disabled;
            }

            var pDebugEngines = stackalloc Guid[1];
            pDebugEngines[0] = DebuggerGuids.DebugEngine;

            uint pid = RDebugPortSupplier.GetProcessId(evaluator.Session.Id);

            var debugTarget = new VsDebugTargetInfo2 {
                cbSize = (uint)Marshal.SizeOf<VsDebugTargetInfo2>(),

                dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_AlreadyRunning,
                guidPortSupplier = DebuggerGuids.PortSupplier,
                bstrPortName = "dummy", // doesn't actually matter, but must not be an empty string
                guidLaunchDebugEngine = DebuggerGuids.DebugEngine,
                dwDebugEngineCount = 1,
                pDebugEngines = (IntPtr)pDebugEngines,
                //LaunchFlags = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_DetachOnStop,

                // R debug port provider represents sessions as pseudo-processes with process ID mapped
                // to session ID. For LaunchDebugTargets2 to attach a process by ID rather than by name,
                // dwProcessId must be set accordingly, _and_ bstrExe must be set to \0 + hex ID.
                dwProcessId = pid,
                bstrExe = (char)0 + "0x" + pid.ToString("X"),
            };

            var pDebugTarget = stackalloc byte[Marshal.SizeOf(debugTarget)];
            Marshal.StructureToPtr(debugTarget, (IntPtr)pDebugTarget, false);

            Marshal.ThrowExceptionForHR(debugger.LaunchDebugTargets2(1, (IntPtr)pDebugTarget));

            return CommandResult.Executed;
        }

        //protected override void Dispose(bool disposing) {
        //    if (_replWindow != null) {
        //        _replWindow.Dispose();
        //        _replWindow = null;
        //    }

        //    base.Dispose(disposing);
        //}
    }
}
