// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflowOperations : IDisposable {

        void ExecuteExpression(string expression);
        void ExecuteCurrentExpression(ITextView textView, Action<ITextView, ITextBuffer, int> formatDocument);

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
        void PositionCaretAtPrompt();
        void ClearPendingInputs();
        Task<ExecutionResult> ResetAsync();
    }
}