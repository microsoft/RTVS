using System.Collections.Generic;
using Microsoft.Languages.Editor.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    //[Export(typeof(ICommandFactory))]
    //[ContentType(MdContentTypeDefinition.ContentType)]
    internal class MdCommandFactory : ICommandFactory {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var commands = new List<ICommand>();
            return commands;
        }
    }
}
