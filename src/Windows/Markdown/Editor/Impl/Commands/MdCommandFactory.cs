// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Preview.Commands;
using Microsoft.Markdown.Editor.Publishing.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Commands {
    [Export(typeof(ICommandFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class MdCommandFactory : ICommandFactory {
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        [ImportingConstructor]
        public MdCommandFactory(ICoreShell coreShell) {
            _services = coreShell.Services;
            _workflowProvider = _services.GetService<IRInteractiveWorkflowProvider>();
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var commands = new List<ICommand>() {
                new RunRChunkCommand(textView, _workflowProvider.GetOrCreate()),
                new GotoBraceCommand(textView, textBuffer, _services),
                new PreviewHtmlCommand(textView, _workflowProvider, _services),
                new PreviewPdfCommand(textView, _workflowProvider, _services),
                new PreviewWordCommand(textView, _workflowProvider, _services),
                new AutomaticSyncCommand(textView, _services),
                new RunCurrentChunkCommand(textView, _services),
                new RunAllChunksAboveCommand(textView, _services)
            };
            return commands;
        }
    }
}
