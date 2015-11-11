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
    /// 'Help on ...' command appears in the editor context menu.
    /// </summary>
    /// <remarks>
    /// Since command changes its name we have to make it package command
    /// since VS IDE no longer handles changing command names via OLE
    /// command target - it never calls IOlecommandTarget::QueryStatus
    /// with OLECMDTEXTF_NAME requesting changing names.
    /// </remarks>
    internal sealed class ShowHelpOnCurrentCommand : PackageCommand {
        public ShowHelpOnCurrentCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent) { }

        protected override void SetStatus() {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                Enabled = true;
                Text = string.Format(CultureInfo.InvariantCulture, Resources.OpenFunctionHelp, item);
            }
            else {
                Enabled = false;
            }
        }

        protected override async void Handle() {
            var rSessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            IReadOnlyDictionary<int, IRSession> sessions = rSessionProvider.GetSessions();
            IRSession session = sessions.Values.FirstOrDefault();
            if(session != null) { 
                string item = GetItemUnderCaret();
                if (item != null) {
                    await TaskUtilities.SwitchToBackgroundThread();
                    session.ScheduleEvaluation(async (e) => {
                        REvaluationResult result = await e.EvaluateAsync("?" + item);
                        if(string.IsNullOrEmpty(result.StringResult) || result.StringResult == "NA") {
                            // Help page not found. This may happen when name is valid
                            // but it comesn from a library that hasn't been loaded yet
                            // such as when user requests help for an item in the code
                            // editor while code has never been executed. Try wider search.
                            result = await e.EvaluateAsync("??" + item);
                        }
                    });
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
            int end = 0;
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
