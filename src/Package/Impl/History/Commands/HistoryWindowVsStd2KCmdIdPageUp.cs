using System;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdPageUp : NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdPageUp(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.PAGEUP) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.ScrollPageUp();
            return CommandResult.Executed;
        }
    }
}