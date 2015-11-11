using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
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
        public ShowHelpOnCurrentCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent) { }

        protected override void SetStatus() {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                Enabled = true;
                Text = string.Format(CultureInfo.InvariantCulture, Resources.OpenFunctionHelp, item);
            } else {
                Enabled = false;
            }
        }

        protected override async void Handle() {
            var rSessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            IReadOnlyDictionary<int, IRSession> sessions = rSessionProvider.GetSessions();
            IRSession session = sessions.Values.FirstOrDefault();
            if (session != null) {
                // Fetch identifier under the cursor
                string item = GetItemUnderCaret();
                if (item != null && item.Length < MaxHelpItemLength) {
                    // Go to background thread
                    await TaskUtilities.SwitchToBackgroundThread();
                    
                    // First check if expression can be evaluated. If result is non-empty
                    // then R knows about the item and '?item' interaction will succed.
                    // If response is empty then we'll try '??item' instead.
                    string prefix = "?";
                    using (IRSessionEvaluation evaluation = await session.BeginEvaluationAsync(isMutating: false)) {
                        REvaluationResult result = await evaluation.EvaluateAsync(prefix + item + Environment.NewLine);
                        if (string.IsNullOrEmpty(result.StringResult) || result.StringResult == "NA") {
                            prefix = "??";
                        }
                    }

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
                            }
                        }
                        break;
                    }
                }
            }
        }

        private string GetItemUnderCaret() {
            ITextView textView = ViewUtilities.ActiveTextView;
            if (textView != null && !textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                string lineText = line.GetText();
                return GetItem(lineText, position.Position - line.Start);
            }
            return string.Empty;
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
