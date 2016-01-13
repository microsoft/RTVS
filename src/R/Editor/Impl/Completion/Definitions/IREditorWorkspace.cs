using System;

namespace Microsoft.R.Editor.Completion.Definitions {
    /// <summary>
    /// Represents R workspace to the editor
    /// </summary>
    public interface IREditorWorkspace {
        /// <summary>
        /// Evaluates given R expression and calls the supplied
        /// callback when the results is ready.
        /// </summary>
        void EvaluateExpression(string expression, Action<string, object> resultCallback, object callbackParameter);

        /// <summary>
        /// Fires when workspace changes such as when variables added or removed,
        /// packages loaded and so on.
        /// </summary>
        event EventHandler Changed;
    }
}
