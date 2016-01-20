using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IRInteractiveSession {
        IRHistory History { get; }
        IRSession RSession { get; }
        IInteractiveWindow InteractiveWindow { get; }
        IInteractiveEvaluator GetOrCreateEvaluator(int instanceId);

        void ExecuteExpression(string expression);
        void ExecuteCurrentExpression(ITextView textView);

        /// <summary>
        /// Enqueues the provided code for execution.  If there's no current execution the code is
        /// inserted at the caret position.  Otherwise the code is stored for when the current
        /// execution is completed.
        /// 
        /// If the current input becomes complete after inserting the code then the input is executed.  
        /// 
        /// If the code is not complete and addNewLine is true then a new line character is appended 
        /// to the end of the input.
        /// </summary>
        /// <param name="expression">The code to be inserted</param>
        /// <param name="addNewLine">True to add a new line on non-complete inputs.</param>
        void EnqueueExpression(string expression, bool addNewLine);
        void ReplaceCurrentExpression(string replaceWith);
        void ClearPendingInputs();
    }
}