// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    public abstract class RSuggestedActionBase : SuggestedActionBase {
        private readonly IRInteractiveWorkflow _workflow;
        private static Task _runningAction;

        protected RSuggestedActionBase(ITextView textView, ITextBuffer textBuffer, IRInteractiveWorkflow workflow, int position, string displayText)
            : base(textBuffer, textView, position, displayText) {
            _workflow = workflow;
        }

        public override bool HasActionSets => _runningAction == null && base.HasActionSets;

        protected void SubmitToInteractive(string command, CancellationToken cancellationToken) {
            if (_workflow.RSession.IsHostRunning) {
                _runningAction = SubmitToInteractiveAsync(command, cancellationToken);
            }
        }

        private async Task SubmitToInteractiveAsync(string command, CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                using (var eval = await _workflow.RSession.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
                    await eval.RespondAsync(command);
                }
            } catch(OperationCanceledException) {
            } finally {
                await _workflow.Shell.SwitchToMainThreadAsync();
                _runningAction = null;
            }
        }
    }
}
