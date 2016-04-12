// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    public abstract class RSuggestedActionBase : SuggestedActionBase {
        private static Task _runningAction;

        protected RSuggestedActionBase(ITextView textView, ITextBuffer textBuffer, int position, string displayText)
            : base(textBuffer, textView, position, displayText) {
        }

        public override bool HasActionSets {
            get {
                return _runningAction == null && base.HasActionSets;
            }
        }

        protected void SubmitToInteractive(string command, CancellationToken cancellationToken) {
            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, null);
            if (session != null && session.IsHostRunning) {
                _runningAction = Task.Run(async () => {
                    try {
                        using (var eval = await session.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
                            await eval.RespondAsync(command);
                        }
                    } finally {
                        EditorShell.DispatchOnUIThread(() => _runningAction = null);
                    }
                });

                _runningAction.SilenceException<OperationCanceledException>()
                              .SilenceException<MessageTransportException>();
            }
        }
    }
}
