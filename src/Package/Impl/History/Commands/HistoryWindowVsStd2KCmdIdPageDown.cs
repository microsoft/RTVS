using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdPageDown: NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdPageDown(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.PAGEDN) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.ScrollPageDown();
            return CommandResult.Executed;
        }
    }
}