using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller.Command
{
    [ExcludeFromCodeCoverage]
    public class DisabledCommand : ViewCommand, ICommand
    {
        public DisabledCommand(ITextView textView, Guid group, int id)
            : base(textView, group, id, false)
        {
        }

        public DisabledCommand(ITextView textView, CommandId[] ids)
            : base(textView, ids, true)
        {
        }

        CommandStatus ICommandTarget.Status(Guid group, int id)
        {
            return CommandStatus.Supported;
        }
    }
}
