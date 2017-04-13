// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controllers;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Comments;
using Microsoft.R.Editor.Completions.Documentation;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Navigation.Commands;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Commands {
    [Export(typeof(ICommandFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class RCommandFactory : ICommandFactory {
        private readonly IObjectViewer _objectViewer;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RCommandFactory([Import(AllowDefault = true)] IObjectViewer objectViewer, [Import(AllowDefault = true)] IRInteractiveWorkflowProvider workflowProvider, ICoreShell shell) {
            _objectViewer = objectViewer;
            _workflowProvider = workflowProvider;
            _shell = shell;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var commands = new List<ICommand> {
                new GotoBraceCommand(textView, textBuffer, _shell),
                new CommentCommand(textView, textBuffer, _shell),
                new UncommentCommand(textView, textBuffer, _shell),
                new FormatDocumentCommand(textView, textBuffer, _shell),
                new FormatSelectionCommand(textView, textBuffer, _shell),
                new FormatOnPasteCommand(textView, textBuffer, _shell),
                new SelectWordCommand(textView, textBuffer),
                new RTypingCommandHandler(textView, _shell),
                new RCompletionCommandHandler(textView),
                new PeekDefinitionCommand(textView, textBuffer, _shell.GetService<IPeekBroker>()),
                new InsertRoxygenBlockCommand(textView, textBuffer)
            };

            if (_workflowProvider != null) {
                commands.Add(new GoToDefinitionCommand(textView, textBuffer, _objectViewer, _workflowProvider.GetOrCreate().RSession));
            }

            return commands;
        }
    }
}
