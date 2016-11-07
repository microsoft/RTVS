// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class InteractiveWindowConsole : IConsole {
        private readonly IRInteractiveWorkflow _workflow;

        public InteractiveWindowConsole(IRInteractiveWorkflow workflow) {
            _workflow = workflow;
        }

        public void Write(string text) {
            _workflow.Shell.DispatchOnUIThread(() => _workflow.ActiveWindow?.InteractiveWindow?.WriteErrorLine(text));
        }

        public async Task<bool> PromptYesNoAsync(string text) {
            var result = await _workflow.Shell.ShowMessageAsync(text, MessageButtons.YesNo);
            return result == MessageButtons.Yes;
        }
    }
}