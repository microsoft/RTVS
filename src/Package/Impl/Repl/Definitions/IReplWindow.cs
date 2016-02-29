using System;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IReplWindow : IDisposable {
        bool IsActive { get; }
        void Show(bool activate);
        IVsInteractiveWindow GetInteractiveWindow();

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
        /// <param name="code">The code to be inserted</param>
        /// <param name="addNewLine">True to add a new line on non-complete inputs.</param>
        void EnqueueCode(string code, bool addNewLine);

        void ExecuteCode(string code);
        void ReplaceCurrentExpression(string replaceWith);
        void ExecuteCurrentExpression(ITextView textView);
        void ClearPendingInputs();
    }
}
