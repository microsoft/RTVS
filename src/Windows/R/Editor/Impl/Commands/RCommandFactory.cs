// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Comments;
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
        private readonly IServiceContainer _services;

        [ImportingConstructor]
        public RCommandFactory([Import(AllowDefault = true)] IObjectViewer objectViewer, [Import(AllowDefault = true)] IRInteractiveWorkflowProvider workflowProvider, ICoreShell shell) {
            _objectViewer = objectViewer;
            _workflowProvider = workflowProvider;
            _shell = shell;
            _services = shell.Services;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var commands = new List<ICommand> {
                new GotoBraceCommand(textView, textBuffer, _services),
                new CommentCommand(textView, textBuffer, _services),
                new UncommentCommand(textView, textBuffer, _services),
                new FormatDocumentCommand(textView, textBuffer, _services),
                new FormatSelectionCommand(textView, textBuffer, _services),
                new FormatOnPasteCommand(textView, textBuffer, _services),
                new SelectWordCommand(textView, textBuffer),
                new RTypingCommandHandler(textView, _services),
                new RCompletionCommandHandler(textView),
                new PeekDefinitionCommand(textView, textBuffer, _services.GetService<IPeekBroker>()),
                new InsertRoxygenBlockCommand(textView, textBuffer)
            };

            if (_workflowProvider != null) {
                commands.Add(new GoToDefinitionCommand(textView, textBuffer, _objectViewer, _workflowProvider.GetOrCreate().RSession));
            }

            return commands;
        }
    }
}
