// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.History.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.RHistory {
    [Export(typeof(ICommandFactory))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    internal class VsRHistoryCommandFactory : ICommandFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly IRInteractiveWorkflowVisualProvider _interactiveWorkflowProvider;
        private readonly IContentTypeRegistryService _contentTypeRegistry;
        private readonly IActiveWpfTextViewTracker _textViewTracker;

        [ImportingConstructor]
        public VsRHistoryCommandFactory(IRInteractiveWorkflowVisualProvider interactiveWorkflowProvider,
            IContentTypeRegistryService contentTypeRegistry,
            IActiveWpfTextViewTracker textViewTracker,
            ICoreShell shell) {

            _historyProvider = shell.Services.GetService<IRHistoryProvider>();
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
            _contentTypeRegistry = contentTypeRegistry;
            _textViewTracker = textViewTracker;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var interactiveWorkflow = _interactiveWorkflowProvider.GetOrCreate();
            var sendToReplCommand = new SendHistoryToReplCommand(textView, _historyProvider, interactiveWorkflow);
            var sendToSourceCommand = new SendHistoryToSourceCommand(textView, _historyProvider, interactiveWorkflow, _contentTypeRegistry, _textViewTracker);

            return new ICommand[] {
                new LoadHistoryCommand(textView, _historyProvider, interactiveWorkflow),
                new SaveHistoryCommand(textView, _historyProvider, interactiveWorkflow),
                sendToReplCommand,
                sendToSourceCommand,
                new DeleteSelectedHistoryEntriesCommand(textView, _historyProvider, interactiveWorkflow),
                new DeleteAllHistoryEntriesCommand(textView, _historyProvider, interactiveWorkflow),
                new HistoryWindowVsStd2KCmdIdReturnCommand(textView, sendToReplCommand, sendToSourceCommand),
                new HistoryWindowVsStd97CmdIdSelectAllCommand(textView, _historyProvider, interactiveWorkflow),
                new HistoryWindowVsStd2KCmdIdUp(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdUpExt(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdDown(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdDownExt(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdLeftExt(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdRightExt(textView, _historyProvider),
                new HistoryWindowVsStd2KCmdIdHome(textView, _historyProvider),
                new HistoryWindowVsStd2KCmdIdEnd(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdPageUp(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdPageDown(textView, _historyProvider), 
                new ToggleMultilineHistorySelectionCommand(textView, _historyProvider, interactiveWorkflow), 
                new CopySelectedHistoryCommand(textView, _historyProvider, interactiveWorkflow)
            };
        }
    }
}