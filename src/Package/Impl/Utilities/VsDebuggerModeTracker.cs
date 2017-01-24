// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IDebuggerModeTracker))]
    internal class VsDebuggerModeTracker : IDebuggerModeTracker, IVsDebuggerEvents {
        public int OnModeChange(DBGMODE dbgmodeNew) {
            if (IsInBreakMode) {
                LeaveBreakMode?.Invoke(this, EventArgs.Empty);
            }

            IsInBreakMode = dbgmodeNew == DBGMODE.DBGMODE_Break;
            IsDebugging = dbgmodeNew != DBGMODE.DBGMODE_Design;

            if (IsInBreakMode) {
                EnterBreakMode?.Invoke(this, EventArgs.Empty);
            }

            return VSConstants.S_OK;
        }

        public bool IsFocusStolenOnBreak =>
#if VS14
            true;
#else
            false;
#endif

        public bool IsInBreakMode { get; private set; }

        public bool IsDebugging { get; private set; }

        public event EventHandler EnterBreakMode;

        public event EventHandler LeaveBreakMode;
    }
}