// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IDebuggerModeTracker))]
    internal class VsDebuggerModeTracker : IDebuggerModeTracker, IVsDebuggerEvents {
        public int OnModeChange(DBGMODE dbgmodeNew) {
            IsEnteredBreakMode = dbgmodeNew == DBGMODE.DBGMODE_Break;
            IsDebugging = dbgmodeNew != DBGMODE.DBGMODE_Design;
            return VSConstants.S_OK;
        }

        public bool IsEnteredBreakMode { get; private set; }

        public bool IsDebugging { get; private set; }

    }
}