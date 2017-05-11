// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Selection;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class ReplCommandFactory : ICommandFactory {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var shell = VsAppShell.Current;
            var services = shell.Services;
            var interactiveWorkflowProvider = services.GetService<IRInteractiveWorkflowVisualProvider>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            var completionBroker = services.GetService<ICompletionBroker>();
            var editorFactory = services.GetService<IEditorOperationsFactoryService>();

            return new ICommand[] {
                new GotoBraceCommand(textView, textBuffer, services),
                new WorkingDirectoryCommand(interactiveWorkflow),
                new HistoryNavigationCommand(textView, interactiveWorkflow, completionBroker, editorFactory),
                new ReplFormatDocumentCommand(textView, textBuffer, services),
                new FormatSelectionCommand(textView, textBuffer, services),
                new FormatOnPasteCommand(textView, textBuffer, services),
                new SendToReplCommand(textView, interactiveWorkflow),
                new ClearReplCommand(textView, interactiveWorkflow),
                new RTypingCommandHandler(textView, services),
                new RCompletionCommandHandler(textView),
                new ExecuteCurrentCodeCommand(textView, interactiveWorkflow),
                new PasteCurrentCodeCommand(textView, interactiveWorkflow),
                new SelectWordCommand(textView, textBuffer),
            };
        }
    }
}
