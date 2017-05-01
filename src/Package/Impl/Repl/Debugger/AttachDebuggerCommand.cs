// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.R.Debugger.PortSupplier;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal class AttachDebuggerCommand : DebuggerCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;

        public AttachDebuggerCommand(IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(interactiveWorkflow, RPackageCommandId.icmdAttachDebugger, DebuggerCommandVisibility.DesignMode) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        protected AttachDebuggerCommand(IRInteractiveWorkflowVisual interactiveWorkflow, int cmdId, DebuggerCommandVisibility visibility)
            : base(interactiveWorkflow, cmdId, visibility) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        protected override unsafe void Handle() {
            if (!RSession.IsHostRunning) {
                return;
            }

            var debugger = _interactiveWorkflow.Shell.GetService<IVsDebugger2>(typeof(SVsShellDebugger));
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
                bstrPortName = RDebugPortSupplier.PortName,
                guidLaunchDebugEngine = DebuggerGuids.DebugEngine,
                dwDebugEngineCount = 1,
                pDebugEngines = (IntPtr)pDebugEngines,
                dwProcessId = pid,
                bstrExe = RDebugPortSupplier.GetExecutableForAttach(pid),
            };

            var pDebugTarget = stackalloc byte[Marshal.SizeOf(debugTarget)];
            Marshal.StructureToPtr(debugTarget, (IntPtr)pDebugTarget, false);

            Marshal.ThrowExceptionForHR(debugger.LaunchDebugTargets2(1, (IntPtr)pDebugTarget));

            // If we have successfully attached, VS has switched to debugging UI context, which hides
            // the REPL window. Show it again and give it focus.
            _interactiveWorkflow.ActiveWindow?.Container.Show(focus: true, immediate: false);
        }
    }
}
