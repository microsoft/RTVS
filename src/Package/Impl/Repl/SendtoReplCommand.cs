using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Options.R.Editor;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Commands
{
    public sealed class SendtoReplCommand : ViewCommand
    {
        public SendtoReplCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, GuidList.CmdSetGuid, RPackageCommandId.icmdSendToRepl, false)
        {
        }

        public override CommandStatus Status(Guid group, int id)
        {
            if (TextView.Selection.Mode == TextSelectionMode.Stream)
            {
                return CommandStatus.SupportedAndEnabled;
            }

            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            ITextSelection selection = TextView.Selection;
            string selectedText;

            if (selection.StreamSelectionSpan.Length == 0)
            {
                int position = selection.Start.Position;
                ITextSnapshotLine line = TextView.TextBuffer.CurrentSnapshot.GetLineFromPosition(position);
                selectedText = line.GetText();
            }
            else
            {
                selectedText = TextView.Selection.StreamSelectionSpan.GetText();
            }

            // Find Active REPL window
            IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IVsWindowFrame windowFrame;
            shell.FindToolWindow(0, GuidList.ReplWindowGuid, out windowFrame);

            if (windowFrame != null)
            {
                // Send text to REPL
                //windowFrame.GetProperty(__VSFPROPID....)
            }

            return CommandResult.Executed;
        }
    }
}
