// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class InteractiveWindowConsole : IConsole, IDisposable {
        private readonly IRInteractiveWorkflowVisual _workflow;
        private readonly IMainThread _mainThread;
        private readonly DisposeToken _disposeToken;
        private IInteractiveWindowVisualComponent _component;

        public InteractiveWindowConsole(IRInteractiveWorkflowVisual workflow) {
            _workflow = workflow;
            _disposeToken = DisposeToken.Create<InteractiveWindowConsole>();
            _mainThread = _workflow.Shell.MainThread();
        }

        public void WriteError(string text) => WriteAsync(text, true).DoNotWait();

        public void Write(string text) => WriteAsync(text, false).DoNotWait();

        private async Task WriteAsync(string text, bool isError) {
            await _mainThread.SwitchToAsync(_disposeToken.CancellationToken);
            if (_component == null) {
                _component = await _workflow.GetOrCreateVisualComponentAsync();
                _component.Container.Show(focus: false, immediate: false);
            }
            if (isError) {
                _component.InteractiveWindow.WriteError(text);
            } else {
                _component.InteractiveWindow.Write(text);
            }
        }

        public void WriteErrorLine(string text) => WriteError(text + Environment.NewLine);
        public void WriteLine(string text) => Write(text + Environment.NewLine);

        public async Task<bool> PromptYesNoAsync(string text, CancellationToken cancellationToken) {
            using (_disposeToken.Link(ref cancellationToken)) {
                var result = await _workflow.Shell.ShowMessageAsync(text, MessageButtons.YesNo, cancellationToken);
                return result == MessageButtons.Yes;
            }
        }

        public void Dispose() => _disposeToken.TryMarkDisposed();
    }
}