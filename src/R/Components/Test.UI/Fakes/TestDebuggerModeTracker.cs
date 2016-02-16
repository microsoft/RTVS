using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Test.UI.Fakes {
    [Export(typeof(IDebuggerModeTracker))]
    [Export(typeof(TestDebuggerModeTracker))]
    internal sealed class TestDebuggerModeTracker : IDebuggerModeTracker {
        public bool IsEnteredBreakMode { get; set; }
    }
}