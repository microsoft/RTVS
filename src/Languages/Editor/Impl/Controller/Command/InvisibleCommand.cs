using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller.Command {
    [ExcludeFromCodeCoverage]
    public class InvisibleCommand : ViewCommand, ICommand {
        public InvisibleCommand(ITextView textView, Guid group, int id)
            : base(textView, group, id, false) {
        }

        CommandStatus ICommandTarget.Status(Guid group, int id) {
            return CommandStatus.Invisible | CommandStatus.Supported;
        }
    }
}
