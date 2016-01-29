using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdHome : NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdHome(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.HOME, VSConstants.VSStd2KCmdID.BOL) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.ScrollToTop();
            return CommandResult.Executed;
        }
    }
}