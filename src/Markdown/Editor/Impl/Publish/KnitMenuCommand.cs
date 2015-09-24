using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands
{
    internal class KnitMenuCommand : ViewCommand
    {
        public KnitMenuCommand(ITextView textView)
            : base(textView, new CommandId[]
                {
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdKnitHtml),
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdKnitPdf),
                  new CommandId(MdPackageCommandId.MdCmdSetGuid, (int)MdPackageCommandId.icmdKnitWord),
                }, false)
        {
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            return CommandResult.Executed;
        }
    }
}
