using System;
using System.Runtime.InteropServices;
using Microsoft.R.Debugger.Engine;
using Microsoft.R.Debugger.Engine.PortSupplier;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal class AttachDebuggerCommand : DebuggerCommand {
        public AttachDebuggerCommand(IRSessionProvider rSessionProvider)
            : base(rSessionProvider, RPackageCommandId.icmdAttachDebugger, DebuggerCommandVisibility.DesignMode) {
        }

        protected AttachDebuggerCommand(IRSessionProvider rSessionProvider, int cmdId, DebuggerCommandVisibility visibility)
            : base(rSessionProvider, cmdId, visibility) {
        }

        protected unsafe override void Handle() {
            if (!RSession.IsHostRunning) {
                return;
            }

            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger2>(typeof(SVsShellDebugger));
            if (debugger == null) {
                return;
            }

            var pDebugEngines = stackalloc Guid[1];
            pDebugEngines[0] = DebuggerGuids.DebugEngine;

            uint pid = RDebugPortSupplier.GetProcessId(RSession.Id);

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

            // If we have successfully attached, VS has switched to debugging UI context, which hides
            // the REPL window. Show it again and give it focus.
            IVsWindowFrame frame = ReplWindow.FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            if (frame != null) {
                frame.Show();
            }
        }
    }
}
