using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    public sealed class HistoryNavigationCommand : ViewCommand
    {
        public HistoryNavigationCommand(ITextView textView) :
            base(textView, new CommandId[] {
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN)
            }, false)
        {
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            if (id == (int)VSConstants.VSStd2KCmdID.UP)
            {
                IVsInteractiveWindow interactive = ReplWindow.Current.GetInteractiveWindow();
                interactive.InteractiveWindow.Operations.HistoryPrevious();
            }
            else if (id == (int)VSConstants.VSStd2KCmdID.DOWN)
            {
                IVsInteractiveWindow interactive = ReplWindow.Current.GetInteractiveWindow();
                interactive.InteractiveWindow.Operations.HistoryNext();
            }

            return CommandResult.Executed;
        }
    }
}
