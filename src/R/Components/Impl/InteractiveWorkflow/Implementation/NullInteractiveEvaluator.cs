// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.InteractiveWindow;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class NullInteractiveEvaluator : IInteractiveEvaluator {
        public void Dispose() { }

        public Task<ExecutionResult> InitializeAsync() => ExecutionResult.Failed;

        public Task<ExecutionResult> ResetAsync(bool initialize = true) {
            return ExecutionResult.Failed;
        }

        public bool CanExecuteCode(string text) => false;

        public Task<ExecutionResult> ExecuteCodeAsync(string text) => ExecutionResult.Failed;

        public string FormatClipboard() => null;

        public void AbortExecution() { }

        public string GetPrompt() => string.Empty;

        public IInteractiveWindow CurrentWindow { get; set; }
    }
}