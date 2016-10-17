// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class InteractiveWindowConsole : IConsole {
        private readonly ICoreShell _coreShell;
        private readonly Lazy<IRInteractiveWorkflow> _workflowLazy;

        public InteractiveWindowConsole(ICoreShell coreShell, Lazy<IRInteractiveWorkflow> workflowLazy) {
            _coreShell = coreShell;
            _workflowLazy = workflowLazy;
        }

        public void Write(string text) {
            _coreShell.DispatchOnUIThread(() => _workflowLazy.Value.ActiveWindow?.InteractiveWindow?.WriteErrorLine(text));
        }

        public async Task<bool> PromptYesNoAsync(string text) {
            var result = await _coreShell.ShowMessageAsync(text, MessageButtons.YesNo);
            return result == MessageButtons.Yes;
        }
    }
}