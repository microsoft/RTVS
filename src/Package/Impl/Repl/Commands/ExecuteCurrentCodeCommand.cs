using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    class ExecuteCurrentCodeCommand : RExecuteCommand {

        public ExecuteCurrentCodeCommand(ITextView textView, IRInteractiveSession interactiveSession) :
            base(textView, interactiveSession, new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRexecuteReplCmd)) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var window = InteractiveSession.InteractiveWindow;
            if (window != null) {
                var text = GetText(window);

                if (text != null) {
                    InteractiveSession.EnqueueExpression(text, false);
                }

                return CommandResult.Executed;
            }
            return CommandResult.Disabled;
        }
    }
}
