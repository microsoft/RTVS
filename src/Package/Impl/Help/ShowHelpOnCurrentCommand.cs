using System;
using System.Globalization;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class ShowHelpOnCurrentCommand : ViewCommand, IDynamicCommandTarget {
        public ShowHelpOnCurrentCommand(ITextView textView) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent), false) { }

        public string GetCommandName(Guid group, int id) {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                return string.Format(CultureInfo.InvariantCulture, Resources.OpenFunctionHelp, item);
            }
            return null;
        }

        public override CommandStatus Status(Guid group, int id) {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (outputArg != null) {
                string item = GetItemUnderCaret();
                if (!string.IsNullOrEmpty(item)) {
                    outputArg = item;
                }
            }
            return CommandResult.Executed;
        }

        private string GetItemUnderCaret() {
            if (!TextView.Caret.InVirtualSpace) {
                SnapshotPoint position = TextView.Caret.Position.BufferPosition;
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
