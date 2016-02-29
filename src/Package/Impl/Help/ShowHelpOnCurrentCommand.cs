using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
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
    internal sealed class ShowHelpOnCurrentCommand : PackageCommand {
        private const int MaxHelpItemLength = 128;
        public ShowHelpOnCurrentCommand(IRSessionProvider sessionProvider, IActiveWpfTextViewTracker textViewTracker) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent) {
            _sessionProvider = sessionProvider;
            _textViewTracker = textViewTracker;
        }

        private IRSessionProvider _sessionProvider;
        private IActiveWpfTextViewTracker _textViewTracker;

        internal override void SetStatus() {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                Enabled = true;
                Text = string.Format(CultureInfo.InvariantCulture, Resources.OpenFunctionHelp, item);
            } else {
                Enabled = false;
            }
        }

        internal override async void Handle() {
            try {
                IRSession session = _sessionProvider.GetInteractiveWindowRSession();
                if (session != null) {
                    // Fetch identifier under the cursor
                    string item = GetItemUnderCaret();
                    if (item != null && item.Length < MaxHelpItemLength) {
                        // First check if expression can be evaluated. If result is non-empty
                        // then R knows about the item and '?item' interaction will succed.
                        // If response is empty then we'll try '??item' instead.
                        string prefix = "?";
                        try {
                            using (IRSessionEvaluation evaluation = await session.BeginEvaluationAsync(isMutating: false)) {
                                REvaluationResult result = await evaluation.EvaluateAsync(prefix + item + Environment.NewLine);
                                if (result.ParseStatus == RParseStatus.OK && string.IsNullOrEmpty(result.Error)) {
                                    if (string.IsNullOrEmpty(result.StringResult) || result.StringResult == "NA") {
                                        prefix = "??";
                                    }
                                } else {
                                    // Parsing or other errors, bail out
                                    Debug.Assert(false,
                                        string.Format(CultureInfo.InvariantCulture,
                                        "Evaluation of help expression failed. Error: {0}, Status: {1}", result.Error, result.ParseStatus));
                                    return;
                                }
                            }
                        } catch (RException) { } catch (OperationCanceledException) { }

                        // Now actually request the help. First call may throw since 'starting help server...'
                        // message in REPL is actually an error (comes in red) so we'll get RException.
                        int retries = 0;
                        while (retries < 3) {
                            using (IRSessionInteraction interaction = await session.BeginInteractionAsync(isVisible: false)) {
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
            if (textView != null && !textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                string lineText = line.GetText();
                return GetItem(lineText, position.Position - line.Start);
            }
            return string.Empty;
        }

        private ITextView GetActiveView() {
            ITextView textView = ReplWindow.Current.GetInteractiveWindow().InteractiveWindow.TextView;
            if (textView != null && textView.HasAggregateFocus) {
                return textView;
            }
            textView = _textViewTracker.GetLastActiveTextView(RContentTypeDefinition.ContentType);
            if (textView != null) {
                return textView;
            }
            return null;
        }

        private string GetItem(string lineText, int position) {
            int start = 0;
            int end = lineText.Length;
            for (int i = position - 1; i >= 0; i--) {
                char ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    start = i + 1;
                    break;
                }
            }
            for (int i = position; i < lineText.Length; i++) {
                char ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    end = i;
                    break;
                }
            }

            if (end > start) {
                return lineText.Substring(start, end - start);
            }

            return string.Empty;
        }
    }
}
