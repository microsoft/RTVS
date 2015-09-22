using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Commands
{
    public sealed class SendToReplCommand : ViewCommand, IVsWindowFrameEvents
    {
        private uint _windowFrameEventsCookie;
        private IVsInteractiveWindow _lastUsedReplWindow;

        public SendToReplCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, new CommandId[] {
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OPENLINEABOVE),
                new CommandId(GuidList.CmdSetGuid, RPackageCommandId.icmdSendToRepl)
            }, false)
        {
            IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
            _windowFrameEventsCookie = shell.AdviseWindowFrameEvents(this);
        }

        public override CommandStatus Status(Guid group, int id)
        {
            if (group == VSConstants.VSStd2K && id == (int)VSConstants.VSStd2KCmdID.OPENLINEABOVE)
            {
                if (!REditorSettings.SendToReplOnCtrlEnter)
                    return CommandStatus.NotSupported;
            }

            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            if (group == VSConstants.VSStd2K && id == (int)VSConstants.VSStd2KCmdID.OPENLINEABOVE)
            {
                if (!REditorSettings.SendToReplOnCtrlEnter)
                    return CommandResult.NotSupported;
            }

            ITextSelection selection = TextView.Selection;
            ITextSnapshot snapshot = TextView.TextBuffer.CurrentSnapshot;
            string selectedText;
            ITextSnapshotLine line = null;

            if (selection.StreamSelectionSpan.Length == 0)
            {
                int position = selection.Start.Position;
                line = snapshot.GetLineFromPosition(position);
                selectedText = line.GetText();
            }
            else
            {
                selectedText = TextView.Selection.StreamSelectionSpan.GetText();
            }

            // Send text to REPL
            if (_lastUsedReplWindow == null)
            {
                IVsWindowFrame frame;
                IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                Guid persistenceSlot = GuidList.ReplInteractiveWindowProviderGuid;
                shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fForceCreate, ref persistenceSlot, out frame);
                frame.Show();
            }

            if (_lastUsedReplWindow != null)
            {
                _lastUsedReplWindow.InteractiveWindow.WriteLine(selectedText);

                if (line != null && line.LineNumber < snapshot.LineCount - 1)
                {
                    ITextSnapshotLine nextLine = snapshot.GetLineFromLineNumber(line.LineNumber + 1);
                    TextView.Caret.MoveTo(new SnapshotPoint(snapshot, nextLine.Start));
                }
            }

            return CommandResult.Executed;
        }

        protected override void Dispose(bool disposing)
        {
            if (_windowFrameEventsCookie != 0)
            {
                IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
                shell.UnadviseWindowFrameEvents(_windowFrameEventsCookie);
                _windowFrameEventsCookie = 0;
            }

            _lastUsedReplWindow = null;

            base.Dispose(disposing);
        }

        #region IVsWindowFrameEvents
        public void OnFrameCreated(IVsWindowFrame frame)
        {
        }

        public void OnFrameDestroyed(IVsWindowFrame frame)
        {
            if (_lastUsedReplWindow == frame)
            {
                _lastUsedReplWindow = null;
            }
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
        {
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
        {
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            // Track last recently used REPL window
            if (!CheckReplFrame(oldFrame))
            {
                CheckReplFrame(newFrame);
            }
        }
        #endregion

        private bool CheckReplFrame(IVsWindowFrame frame)
        {
            Guid property;

            if (frame != null)
            {
                frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out property);
                if (property == GuidList.ReplInteractiveWindowProviderGuid)
                {
                    object docView;
                    frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
                    _lastUsedReplWindow = docView as IVsInteractiveWindow;
                    return _lastUsedReplWindow != null;
                }
            }

            return false;
        }
    }
}
