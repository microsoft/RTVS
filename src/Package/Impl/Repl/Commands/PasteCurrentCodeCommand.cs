using System;
using System.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    class PasteCurrentCodeCommand : RExecuteCommand {

        public PasteCurrentCodeCommand(ITextView textView, IRInteractiveSession interactiveSession) :
            base(textView, interactiveSession, new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPasteReplCmd)) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var window = InteractiveSession.InteractiveWindow;
            if (window != null) {
                string text = GetText(window);

                if (text != null) {
                    var curBuffer = window.CurrentLanguageBuffer;
                    curBuffer.Insert(curBuffer.CurrentSnapshot.Length, text);

                    // Caret is where the user clicked, move it to the end of the buffer
                    var curSnapshot = TextView.TextBuffer.CurrentSnapshot;
                    TextView.Caret.MoveTo(new SnapshotPoint(curSnapshot, curSnapshot.Length));
                    TextView.Caret.EnsureVisible();
                    return CommandResult.Executed;
                }
            }
            return CommandResult.Disabled;
        }
    }
}
