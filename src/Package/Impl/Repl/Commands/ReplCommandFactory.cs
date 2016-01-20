using System.Collections.Generic;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class ReplCommandFactory : ICommandFactory {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var exportProvider = VsAppShell.Current.ExportProvider;
            var interactiveSessionProvider = exportProvider.GetExportedValue<IRInteractiveSessionProvider>();
            var interactiveSession = interactiveSessionProvider.GetOrCreate();
            var completionBroker = exportProvider.GetExportedValue<ICompletionBroker>();
            var editorFactory = exportProvider.GetExportedValue<IEditorOperationsFactoryService>();

            return new ICommand[] {
                new GotoBraceCommand(textView, textBuffer),
                new WorkingDirectoryCommand(),
                new HistoryNavigationCommand(textView, interactiveSession, completionBroker, editorFactory),
                new ReplFormatDocumentCommand(textView, textBuffer),
                new FormatSelectionCommand(textView, textBuffer),
                new FormatOnPasteCommand(textView, textBuffer),
                new SendToReplCommand(textView, interactiveSession),
                new RTypingCommandHandler(textView),
                new RCompletionCommandHandler(textView),
                new ExecuteCurrentCodeCommand(textView, interactiveSession),
                new PasteCurrentCodeCommand(textView, interactiveSession)
            };
        }
    }
}
