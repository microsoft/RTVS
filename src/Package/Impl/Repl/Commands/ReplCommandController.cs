using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    /// <summary>
    /// Main HTML editor command controller
    /// </summary>
    public class ReplCommandController : ViewController {
        private ICompletionBroker _completionBroker;

        public ReplCommandController(ITextView textView, ITextBuffer textBuffer)
            : base(textView, textBuffer) {
            ServiceManager.AddService<ReplCommandController>(this, textView);
        }

        public static ReplCommandController Attach(ITextView textView, ITextBuffer textBuffer) {
            ReplCommandController controller = FromTextView(textView);
            if (controller == null) {
                controller = new ReplCommandController(textView, textBuffer);
            }

            return controller;
        }

        public static ReplCommandController FromTextView(ITextView textView) {
            return ServiceManager.GetService<ReplCommandController>(textView);
        }

        public override void BuildCommandSet() {
            if (EditorShell.Current.CompositionService != null) {
                var factory = new ReplCommandFactory();
                var commands = factory.GetCommands(TextView, TextBuffer);
                AddCommandSet(commands);
            }
        }

        public override CommandStatus Status(Guid group, int id) {
            if ((NonRoutedStatus(group, id, null) & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled
                && !IsCompletionCommand(group, id)) {
                return CommandStatus.SupportedAndEnabled;
            }

            return base.Status(group, id);
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                RCompletionController controller = RCompletionController.FromTextView(TextView);
                if (controller != null) {
                    if (id == (int)VSConstants.VSStd2KCmdID.TAB) {
                        return HandleTab(controller);
                        // If completion is up, commit it
                    } else if (id == (int)VSConstants.VSStd2KCmdID.RETURN) {
                        return HandleEnter(controller);
                    } else if (id == (int)VSConstants.VSStd2KCmdID.CANCEL) {
                        HandleCancel(controller);
                        // Allow VS to continue processing cancel
                    }
                }
            }

            return base.Invoke(group, id, inputArg, ref outputArg);
        }
        private CommandResult HandleTab(RCompletionController controller) {
            // If completion is up, commit it
            if (controller.HasActiveCompletionSession) {
                controller.CommitCompletionSession();
                controller.DismissAllSessions();
            } else {
                controller.DismissAllSessions();
                controller.ShowCompletion(autoShownCompletion: true);
            }
            return CommandResult.Executed;
        }

        private CommandResult HandleEnter(RCompletionController controller) {
            // If completion is up, commit it
            if (controller.HasActiveCompletionSession) {
                // Check for exact match. If applicable span is 'x' and completion is 'x'
                // then we don't complete and rather execute. If span is 'x' while
                // current completion entry is 'X11' then we complete depending on
                // the 'complete on enter' setting.
                try {
                    ICompletionSession session = controller.CompletionSession;
                    CompletionSet set = session.SelectedCompletionSet;
                    ITrackingSpan span = set.ApplicableTo;
                    ITextSnapshot snapshot = span.TextBuffer.CurrentSnapshot;
                    string spanText = snapshot.GetText(span.GetSpan(snapshot));
                    if (spanText != set.SelectionStatus.Completion.InsertionText) {
                        controller.CommitCompletionSession();
                        controller.DismissAllSessions();
                        return CommandResult.Executed;
                    }
                } catch (Exception) { }
            }

            controller.DismissAllSessions();
            ReplWindow.Current.ExecuteCurrentExpression(TextView);
            return CommandResult.Executed;
        }

        private void HandleCancel(RCompletionController controller) {
            IVsUIShell uiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            Guid gmdSet = RGuidList.RCmdSetGuid;
            object o = new object();
            // Post interrupt command which knows if it can interrupt R or not
            uiShell.PostExecCommand(ref gmdSet, RPackageCommandId.icmdInterruptR, 0, ref o);
        }

        /// <summary>
        /// Determines if command is one of the completion commands
        /// </summary>
        private bool IsCompletionCommand(Guid group, int id) {
            ICommand cmd = Find(group, id);
            return cmd is RCompletionCommandHandler;
        }

        private ICompletionBroker CompletionBroker {
            get {
                if (_completionBroker == null) {
                    _completionBroker = EditorShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
                }

                return _completionBroker;
            }
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (TextView != null) {
                ServiceManager.RemoveService<ReplCommandController>(TextView);
            }

            base.Dispose(disposing);
        }
    }
}