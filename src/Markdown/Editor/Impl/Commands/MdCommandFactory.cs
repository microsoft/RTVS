using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Commands
{
    [Export(typeof(ICommandFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class VsCommandFactory : ICommandFactory
    {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer)
        {
            var commands = new List<ICommand>();

            return commands;
        }
    }
}
