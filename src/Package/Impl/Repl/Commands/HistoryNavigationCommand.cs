using System;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    public sealed class HistoryNavigationCommand : ViewCommand
    {
        [Import]
        private ICompletionBroker _completionBroker { get; set; }

        public HistoryNavigationCommand(ITextView textView) :
            base(textView, new CommandId[] {
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN)
            }, false)
        {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);
        }

        public override CommandStatus Status(Guid group, int id)
        {
            if(_completionBroker.IsCompletionActive(TextView))
            {
                return CommandStatus.NotSupported;
            }

            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            if (_completionBroker.IsCompletionActive(TextView))
            {
                return CommandResult.NotSupported;
            }

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
