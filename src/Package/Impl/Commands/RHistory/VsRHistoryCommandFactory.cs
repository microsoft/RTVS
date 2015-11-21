using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.History.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.RHistory {
    [Export(typeof(ICommandFactory))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    internal class VsRHistoryCommandFactory : ICommandFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly IContentTypeRegistryService _contentTypeRegistry;
        private readonly IActiveWpfTextViewTracker _textViewTracker;

        [ImportingConstructor]
        public VsRHistoryCommandFactory(IRHistoryProvider historyProvider, IContentTypeRegistryService contentTypeRegistry, IActiveWpfTextViewTracker textViewTracker) {
            _historyProvider = historyProvider;
            _contentTypeRegistry = contentTypeRegistry;
            _textViewTracker = textViewTracker;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            return new List<ICommand> {
                new LoadHistoryCommand(textView, _historyProvider),
                new SaveHistoryCommand(textView, _historyProvider),
                new SendHistoryToReplCommand(textView, _historyProvider),
                new SendHistoryToSourceCommand(textView, _historyProvider, _contentTypeRegistry, _textViewTracker)
            };
        }
    }
}