// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Help {
    /// <summary>
    /// 'Help on ...' command that appears in the editor context menu.
    /// </summary>
    /// <remarks>
    /// Since command changes its name we have to make it package command
    /// since VS IDE no longer handles changing command names via OLE
    /// command target - it never calls IOlecommandTarget::QueryStatus
    /// with OLECMDTEXTF_NAME requesting changing names.
    /// </remarks>
    internal sealed class ShowHelpOnCurrentCommand : PackageCommand {
        private const int MaxHelpItemLength = 128;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IActiveWpfTextViewTracker _textViewTracker;

        private IActiveRInteractiveWindowTracker _activeReplTracker;
        public ShowHelpOnCurrentCommand(IRInteractiveWorkflow workflow, IActiveWpfTextViewTracker textViewTracker, IActiveRInteractiveWindowTracker activeReplTracker) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent) {
            _activeReplTracker = activeReplTracker;
            _workflow = workflow;
            _textViewTracker = textViewTracker;
        }

        protected override void SetStatus() {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                Enabled = true;
                Text = string.Format(CultureInfo.InvariantCulture, Resources.OpenFunctionHelp, item);
            } else {
                Enabled = false;
            }
        }

        protected override void Handle() {
            try {
                if (!_workflow.RSession.IsHostRunning) {
                    return;
                }

                // Fetch identifier under the cursor
                string item = GetItemUnderCaret();
                if (item == null || item.Length >= MaxHelpItemLength) {
                    return;
                }

                _workflow.RSession.ExecuteAsync($"rtvs:::show_help({item.ToRStringLiteral()})")
                    .SilenceException<RException>()
                    .SilenceException<MessageTransportException>()
                    .DoNotWait();
            } catch (Exception ex) {
                Debug.Assert(false, string.Format(CultureInfo.InvariantCulture, "Help on current item failed. Exception: {0}", ex.Message));
                // Catch everything so exceptions don't leave the async void method
                if (ex.IsCriticalException()) {
                    throw;
                }
            }
        }

        private string GetItemUnderCaret() {
            ITextView textView = GetActiveView();
            if (textView != null) {
                Span span;
                return textView.GetIdentifierUnderCaret(out span);
            }
            return string.Empty;
        }

        private ITextView GetActiveView() {
            var activeReplWindow = _activeReplTracker.LastActiveWindow;
            if (activeReplWindow != null && _activeReplTracker.IsActive) {
                return activeReplWindow.InteractiveWindow.TextView;
            }
            return _textViewTracker.LastActiveTextView;
        }
    }
}
