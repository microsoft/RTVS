using System;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdDown : NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdDown(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.DOWN) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.SelectNextHistoryEntry();
            return CommandResult.Executed;
        }
    }
}