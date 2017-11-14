// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class InteractiveWindowOperationsMock : IInteractiveWindowOperations {
        private ITextBuffer _textBuffer;

        public InteractiveWindowOperationsMock(ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
        }

        public bool Backspace() {
            return true;
        }

        public bool BreakLine() {
            return true;
        }

        public void Cancel() {
        }

        public void ClearHistory() {
        }

        public void ClearView() {
        }

        public void Cut() {
        }

        public bool Delete() {
            return true;
        }

        public void End(bool extendSelection) {
        }

        public void ExecuteInput() {
            _textBuffer.Insert(_textBuffer.CurrentSnapshot.Length, "\r\n");
        }

        public void HistoryNext(string search = null) {
        }

        public void HistoryPrevious(string search = null) {
        }

        public void HistorySearchNext() {
        }

        public void HistorySearchPrevious() {
        }

        public void Home(bool extendSelection) {
        }

        public bool Paste() {
            return true;
        }

        public Task<ExecutionResult> ResetAsync(bool initialize = true) {
            return System.Threading.Tasks.Task.FromResult(ExecutionResult.Success);
        }

        public bool Return() {
            return true;
        }

        public void SelectAll() {
        }

        public bool TrySubmitStandardInput() {
            return true;
        }
    }
}
