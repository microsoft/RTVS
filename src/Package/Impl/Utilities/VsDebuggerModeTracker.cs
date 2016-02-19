using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IDebuggerModeTracker))]
    internal class VsDebuggerModeTracker : IDebuggerModeTracker, IVsDebuggerEvents {
        public int OnModeChange(DBGMODE dbgmodeNew) {
            IsEnteredBreakMode = dbgmodeNew == DBGMODE.DBGMODE_Break;
            return VSConstants.S_OK;
        }

        public bool IsEnteredBreakMode { get; private set; }
    }
}