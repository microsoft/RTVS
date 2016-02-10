namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IDebuggerModeTracker {
        bool IsEnteredBreakMode { get; }
    }
}