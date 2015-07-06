using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.Languages.Editor.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor
{
    [Export(typeof(ICommandFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class HtmlCommandFactory: ICommandFactory
    {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer)
        {
            List<ICommand> commands = new List<ICommand>();

            //commands.Add(new CommentSelectionCommand(textView, textBuffer));
            //commands.Add(new UncommentSelectionCommand(textView, textBuffer));
            //commands.Add(new FormatDocumentCommand(textView, textBuffer));
            //commands.Add(new FormatSelectionCommand(textView, textBuffer));
            //commands.Add(new HtmlTypingCommandHandler(textView));
            //commands.Add(new HtmlCompletionCommandHandler(textView, textBuffer));
            //commands.Add(new HtmlTagMatchCommand(textView, textBuffer));
            //commands.Add(new UnminifyCommand(textView, textBuffer));
            //commands.Add(new WrapWithDivCommand(textView, textBuffer));
            //commands.Add(new HtmlOutlineTagsCommandHandler(textView));

            return commands;
        }
    }
}
