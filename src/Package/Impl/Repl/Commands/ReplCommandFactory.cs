using System.Collections.Generic;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    internal sealed class ReplCommandFactory: ICommandFactory
    {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer)
        {
            List<ICommand> commands = new List<ICommand>();

            commands.Add(new HistoryNavigationCommand(textView));
            commands.Add(new ReplFormatDocumentCommand(textView, textBuffer));
            commands.Add(new FormatSelectionCommand(textView, textBuffer));
            commands.Add(new FormatOnPasteCommand(textView, textBuffer));
            commands.Add(new RTypingCommandHandler(textView));
            commands.Add(new RCompletionCommandHandler(textView));

            return commands;
        }
    }
}
