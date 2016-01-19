using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryProvider))]
    internal class RHistoryProvider : IRHistoryProvider {
        private readonly ITextEditorFactoryService _textEditorFactory;
        private readonly ITextBufferFactoryService _textBufferFactory;
        private readonly IFileSystem _fileSystem;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IRtfBuilderService _rtfBuilderService;
        private readonly ITextSearchService2 _textSearchService;
        private readonly IContentType _contentType;
        private readonly Dictionary<ITextBuffer, IRHistory> _histories;

        [ImportingConstructor]
        public RHistoryProvider(ITextEditorFactoryService textEditorFactory, ITextBufferFactoryService textBufferFactory, IContentTypeRegistryService contentTypeRegistryService, IFileSystem fileSystem, IEditorOperationsFactoryService editorOperationsFactory, IRtfBuilderService rtfBuilderService, ITextSearchService2 textSearchService) {
            _textEditorFactory = textEditorFactory;
            _textBufferFactory = textBufferFactory;
            _fileSystem = fileSystem;
            _editorOperationsFactory = editorOperationsFactory;
            _rtfBuilderService = rtfBuilderService;
            _textSearchService = textSearchService;
            _rtfBuilderService = rtfBuilderService;
            _contentType = contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
            _histories = new Dictionary<ITextBuffer, IRHistory>();
        }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            IRHistory history;
            return _histories.TryGetValue(textView.TextDataModel.DocumentBuffer, out history) ? history : null;
        }

        public IRHistoryFiltering CreateFiltering(IRHistory history) {
            var textView = GetOrCreateTextView(history);
            return new RHistoryFiltering(history, textView, RToolsSettings.Current, _textSearchService);
        }

        public IWpfTextView GetOrCreateTextView(IRHistory history) {
            return history.GetOrCreateTextView(_textEditorFactory);
        }

        public IRHistory CreateRHistory(IRInteractive rInteractive) {
            var vsUiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            var textBuffer = _textBufferFactory.CreateTextBuffer(_contentType);
            var history = new RHistory(rInteractive, textBuffer, _fileSystem, RToolsSettings.Current, _editorOperationsFactory, _rtfBuilderService, vsUiShell, () => RemoveRHistory(textBuffer));
            _histories[textBuffer] = history;
            return history;
        }

        private void RemoveRHistory(ITextBuffer textBuffer) {
            _histories.Remove(textBuffer);
        }
    }
}