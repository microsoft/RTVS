// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class InteractiveWindowConsole : IConsole {
        private readonly IRInteractiveWorkflow _workflow;
        private IInteractiveWindowVisualComponent _component;

        public InteractiveWindowConsole(IRInteractiveWorkflow workflow) {
            _workflow = workflow;
        }

        public void Write(string text) {
            WriteAsync(text).DoNotWait();
        }

        private async Task WriteAsync(string text) {
            await _workflow.Shell.SwitchToMainThreadAsync();
            if (_component == null) {
                _component = await _workflow.GetOrCreateVisualComponentAsync();
                _component.Container.Show(focus: false, immediate: false);
            }
            _component.InteractiveWindow.WriteError(text);
        }

        public void WriteLine(string text) => Write(text + Environment.NewLine);

        public async Task<bool> PromptYesNoAsync(string text, CancellationToken cancellationToken) {
            var result = await _workflow.Shell.ShowMessageAsync(text, MessageButtons.YesNo, cancellationToken);
            return result == MessageButtons.Yes;
        }
    }
}