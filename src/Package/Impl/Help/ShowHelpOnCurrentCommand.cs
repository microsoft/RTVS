// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
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
    internal sealed class ShowHelpOnCurrentCommand : ViewCommand {
        private const int MaxHelpItemLength = 128;
        private readonly IRInteractiveWorkflow _workflow;

        public ShowHelpOnCurrentCommand(ITextView textView, IRInteractiveWorkflow workflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent), needCheckout: false) {
            _workflow = workflow;
        }

        public override CommandStatus Status(Guid group, int id) {
            Span span;
            var item = TextView.GetIdentifierUnderCaret(out span);
            if (!string.IsNullOrEmpty(item)) {
                //Text = string.Format(CultureInfo.InvariantCulture, Resources.OpenFunctionHelp, item);
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            try {
                if (!_workflow.RSession.IsHostRunning) {
                    return CommandResult.NotSupported;
                }

                // Fetch identifier under the cursor
                Span span;
                var item = TextView.GetIdentifierUnderCaret(out span);
                if (item == null || item.Length >= MaxHelpItemLength) {
                    return CommandResult.NotSupported;
                }

                // First check if expression can be evaluated. If result is non-empty
                // then R knows about the item and '?item' interaction will succed.
                // If response is empty then we'll try '??item' instead.
                string prefix = "?";
                item = AddQuotes(item);
                ShowHelpOnCurrentAsync(prefix, "'" + item + "'").DoNotWait();
            } catch (Exception ex) {
                Debug.Assert(false, string.Format(CultureInfo.InvariantCulture, "Help on current item failed. Exception: {0}", ex.Message));
                // Catch everything so exceptions don't leave the async void method
                if (ex.IsCriticalException()) {
                    throw;
                }
            }
            return CommandResult.Executed;
        }

        private static string AddQuotes(string s) {
            if (s.Length > 0) {
                if (s[0] != '\'') {
                    s = "'" + s;
                }
                if (s[s.Length - 1] != '\'') {
                    s = s + "'";
                }
            }
            return s;
        }

        private async Task ShowHelpOnCurrentAsync(string prefix, string item) {
            try {
                using (IRSessionEvaluation evaluation = await _workflow.RSession.BeginEvaluationAsync(isMutating: false)) {
                    REvaluationResult result = await evaluation.EvaluateAsync(prefix + item + Environment.NewLine);
                    if (result.ParseStatus == RParseStatus.OK &&
                        string.IsNullOrEmpty(result.Error)) {
                        if (string.IsNullOrEmpty(result.StringResult) ||
                            result.StringResult == "NA") {
                            prefix = "??";
                        }
                    } else {
                        // Parsing or other errors, bail out
                        Debug.Assert(false,
                            string.Format(CultureInfo.InvariantCulture,
                            "Evaluation of help expression failed. Error: {0}, Status: {1}", result.Error, result.ParseStatus));
                    }
                }
            } catch (RException) {
            } catch (OperationCanceledException) {
            }

            // Now actually request the help. First call may throw since 'starting help server...'
            // message in REPL is actually an error (comes in red) so we'll get RException.
            int retries = 0;
            while (retries < 3) {
                using (IRSessionInteraction interaction = await _workflow.RSession.BeginInteractionAsync(isVisible: false)) {
                    try {
                        await interaction.RespondAsync(prefix + item + Environment.NewLine);
                    } catch (RException ex) {
                        if ((uint)ex.HResult == 0x80131500) {
                            // Typically 'starting help server...' so try again
                            retries++;
                            continue;
                        }
                    } catch (OperationCanceledException) { }
                }

                break;
            }
        }
    }
}
