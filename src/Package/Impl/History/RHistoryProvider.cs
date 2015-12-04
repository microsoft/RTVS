using System;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.History {
    internal class RHistoryProvider : IRHistoryProvider {
        private const string IntraTextAdornmentBufferKey = "IntraTextAdornmentBuffer";

        private readonly IFileSystem _fileSystem;
        private readonly Lazy<IEditorOperationsFactoryService> _editorOperationsFactoryLazy;
        private readonly IRtfBuilderService _rtfBuilderService;
        private readonly ITextSearchService2 _textSearchService;

        public RHistoryProvider(IFileSystem fileSystem, Lazy<IEditorOperationsFactoryService> editorOperationsFactoryLazy, IRtfBuilderService rtfBuilderService, ITextSearchService2 textSearchService) {
            _fileSystem = fileSystem;
            _editorOperationsFactoryLazy = editorOperationsFactoryLazy;
            _rtfBuilderService = rtfBuilderService;
            _textSearchService = textSearchService;
        }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            return textView.Properties.GetOrCreateSingletonProperty(typeof(RHistory), () => CreateRHistory(textView));
        }

        private RHistory CreateRHistory(ITextView textView) {
            IElisionBuffer elisionBuffer;
            if (!textView.TextViewModel.Properties.TryGetProperty(IntraTextAdornmentBufferKey, out elisionBuffer)) {
                throw new InvalidOperationException("TextView should have PredefinedTextViewRoles.Structured view role");
            }

            var vsUiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            return new RHistory(textView, _fileSystem, _editorOperationsFactoryLazy.Value, elisionBuffer, _rtfBuilderService, _textSearchService, vsUiShell);
        }
    }
}