using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    /// <summary>
    /// Main HTML editor command controller
    /// </summary>
    public class ReplCommandController : ViewController
    {
        private ICompletionBroker _completionBroker;

        public ReplCommandController(ITextView textView, ITextBuffer textBuffer)
            : base(textView, textBuffer)
        {
            ServiceManager.AddService<ReplCommandController>(this, textView);
        }

        public static ReplCommandController Attach(ITextView textView, ITextBuffer textBuffer)
        {
            ReplCommandController controller = FromTextView(textView);
            if (controller == null)
            {
                controller = new ReplCommandController(textView, textBuffer);
            }

            return controller;
        }

        public static ReplCommandController FromTextView(ITextView textView)
        {
            return ServiceManager.GetService<ReplCommandController>(textView);
        }

        public override void BuildCommandSet()
        {
            if (EditorShell.Current.CompositionService != null)
            {
                var factory = new ReplCommandFactory();
                var commands = factory.GetCommands(TextView, TextBuffer);
                AddCommandSet(commands);
            }
        }

        public override CommandStatus Status(Guid group, int id)
        {
            if ((NonRoutedStatus(group, id, null) & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled
                && !IsCompletionCommand(group, id))
            {
                return CommandStatus.SupportedAndEnabled;
            }

            return base.Status(group, id);
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            if (group == VSConstants.VSStd2K)
            {
                if (id == (int)VSConstants.VSStd2KCmdID.TAB)
                {
                    CompletionBroker.DismissAllSessions(TextView);
                    CompletionBroker.TriggerCompletion(TextView);
                    return CommandResult.Executed;
                }
                else if (id == (int)VSConstants.VSStd2KCmdID.RETURN)
                {
                    // execute if the expression is complete
                    ReplWindow.Current.ExecuteCurrentExpression(TextView);
                    return CommandResult.Executed;
                }
            }

            return base.Invoke(group, id, inputArg, ref outputArg);
        }

        /// <summary>
        /// Determines if command is one of the completion commands
        /// </summary>
        private bool IsCompletionCommand(Guid group, int id)
        {
            ICommand cmd = Find(group, id);
            return cmd is RCompletionCommandHandler;
        }

        private ICompletionBroker CompletionBroker
        {
            get
            {
                if (_completionBroker == null)
                {
                    _completionBroker = EditorShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
                }

                return _completionBroker;
            }
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (TextView != null)
            {
                ServiceManager.RemoveService<ReplCommandController>(TextView);
            }

            base.Dispose(disposing);
        }
    }
}