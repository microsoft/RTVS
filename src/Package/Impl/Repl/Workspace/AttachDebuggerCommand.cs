using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Debugger.Engine;
using Microsoft.R.Debugger.Engine.PortSupplier;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class AttachDebuggerCommand : PackageCommand {
        private readonly IRSessionProvider _rSessionProvider;

        public AttachDebuggerCommand(IRSessionProvider rSessionProvider)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdAttachDebugger) {
            _rSessionProvider = rSessionProvider;
        }

        protected override void SetStatus() {
            Enabled = false;

            if (_rSessionProvider.Current == null) {
                return;
            }

            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(SVsShellDebugger));
            if (debugger == null) {
                return;
            }

            var mode = new DBGMODE[1];
            if (debugger.GetMode(mode) < 0 || mode[0] != DBGMODE.DBGMODE_Design) {
                return;
            }

            Enabled = true;
        }


        protected unsafe override void Handle() {
            var session = _rSessionProvider.Current;
            if (session == null) {
                return;
            }

            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger2>(typeof(SVsShellDebugger));
            if (debugger == null) {
                return;
            }

            var pDebugEngines = stackalloc Guid[1];
            pDebugEngines[0] = DebuggerGuids.DebugEngine;

            uint pid = RDebugPortSupplier.GetProcessId(session.Id);

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
