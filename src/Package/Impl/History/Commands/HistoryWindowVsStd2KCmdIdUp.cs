using System;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdUp : NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdUp(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.UP) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.SelectPreviousHistoryEntry();
            return CommandResult.Executed;
        }
    }
}