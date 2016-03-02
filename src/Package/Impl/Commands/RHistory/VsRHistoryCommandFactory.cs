// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands.R;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.History.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.RHistory {
    [Export(typeof(ICommandFactory))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    internal class VsRHistoryCommandFactory : ICommandFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly IContentTypeRegistryService _contentTypeRegistry;
        private readonly IActiveWpfTextViewTracker _textViewTracker;
        private readonly IEditorOperationsFactoryService _editorOperationsService;

        [ImportingConstructor]
        public VsRHistoryCommandFactory(IRHistoryProvider historyProvider,
            IContentTypeRegistryService contentTypeRegistry,
            IActiveWpfTextViewTracker textViewTracker,
            IEditorOperationsFactoryService editorOperationsService) {

            _historyProvider = historyProvider;
            _contentTypeRegistry = contentTypeRegistry;
            _textViewTracker = textViewTracker;
            _editorOperationsService = editorOperationsService;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var sendToReplCommand = new SendHistoryToReplCommand(textView, _historyProvider);
            var sendToSourceCommand = new SendHistoryToSourceCommand(textView, _historyProvider, _contentTypeRegistry, _textViewTracker);

            return new ICommand[] {
                new LoadHistoryCommand(textView, _historyProvider),
                new SaveHistoryCommand(textView, _historyProvider),
                sendToReplCommand,
                sendToSourceCommand,
                new DeleteSelectedHistoryEntriesCommand(textView, _historyProvider),
                new DeleteAllHistoryEntriesCommand(textView, _historyProvider),
                new HistoryWindowVsStd2KCmdIdReturnCommand(textView, sendToReplCommand, sendToSourceCommand),
                new HistoryWindowVsStd97CmdIdSelectAllCommand(textView, _historyProvider),
                new HistoryWindowVsStd2KCmdIdUp(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdDown(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdHome(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdEnd(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdPageUp(textView, _historyProvider), 
                new HistoryWindowVsStd2KCmdIdPageDown(textView, _historyProvider), 
                new ToggleMultilineHistorySelectionCommand(textView, _historyProvider, RToolsSettings.Current), 
                new CopySelectedHistoryCommand(textView, _historyProvider, _editorOperationsService)
            };
        }
    }
}