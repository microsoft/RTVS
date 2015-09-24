using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands
{
    [Export(typeof(ICommandFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class VsCommandFactory : ICommandFactory
    {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer)
        {
            var commands = new List<ICommand>();

            commands.Add(new ShowContextMenuCommand(textView, GuidList.RPackageGuid, GuidList.CmdSetGuid, ContextMenuId.R));
            commands.Add(new GoToFormattingOptionsCommand(textView, textBuffer));
            commands.Add(new SendToReplCommand(textView, textBuffer));

            return commands;
        }
    }
}
