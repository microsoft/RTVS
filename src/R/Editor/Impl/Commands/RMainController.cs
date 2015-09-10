using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands
{
    /// <summary>
    /// Main HTML editor command controller
    /// </summary>
    public class RMainController : ViewController
    {
        private RCompletionController _completionController;

        public RMainController(ITextView textView, ITextBuffer textBuffer)
            : base(textView, textBuffer)
        {
            ServiceManager.AddService<RMainController>(this, textView);
        }

        public static RMainController Attach(ITextView textView, ITextBuffer textBuffer)
        {
            RMainController controller = FromTextView(textView);
            if (controller == null)
            {
                controller = new RMainController(textView, textBuffer);
            }

            return controller;
        }

        public static RMainController FromTextView(ITextView textView)
        {
            return ServiceManager.GetService<RMainController>(textView);
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

        private void DismissAllSessions()
        {
            ICompletionBroker completionBroker = EditorShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
            completionBroker.DismissAllSessions(TextView);
        }

        private ICommandTarget CompletionController
        {
            get
            {
                if (_completionController == null)
                    _completionController = ServiceManager.GetService<RCompletionController>(TextView);

                return _completionController;
            }
        }

        /// <summary>
        /// Determines if command is one of the completion commands
        /// </summary>
        private bool IsCompletionCommand(Guid group, int id)
        {
            ICommand cmd = Find(group, id);
            return cmd is RCompletionCommandHandler;
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (TextView != null)
            {
                ServiceManager.RemoveService<RMainController>(TextView);
            }

            base.Dispose(disposing);
        }
    }
}