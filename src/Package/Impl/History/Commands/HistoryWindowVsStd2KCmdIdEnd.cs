using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdEnd : NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdEnd(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.END, VSConstants.VSStd2KCmdID.EOL) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.ScrollToBottom();
            return CommandResult.Executed;
        }
    }
}