using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryProvider))]
    internal class RHistoryProvider : IRHistoryProvider {
        private const string IntraTextAdornmentBufferKey = "IntraTextAdornmentBuffer";

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            return textView.Properties.GetOrCreateSingletonProperty(typeof(RHistory), () => CreateRHistory(textView));
        }

        private readonly IFileSystem _fileSystem;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IRtfBuilderService _rtfBuilderService;
        private readonly ITextSearchService2 _textSearchService;

        [ImportingConstructor]
        public RHistoryProvider(IFileSystem fileSystem, IEditorOperationsFactoryService editorOperationsFactory, IRtfBuilderService rtfBuilderService, ITextSearchService2 textSearchService) {
            _fileSystem = fileSystem;
            _editorOperationsFactory = editorOperationsFactory;
            _rtfBuilderService = rtfBuilderService;
            _textSearchService = textSearchService;
            _rtfBuilderService = rtfBuilderService;
        }

        private RHistory CreateRHistory(ITextView textView) {
            IElisionBuffer elisionBuffer;
            if (!textView.TextViewModel.Properties.TryGetProperty(IntraTextAdornmentBufferKey, out elisionBuffer)) {
                if (!VsAppShell.Current.IsUnitTestEnvironment) {
                    throw new InvalidOperationException("TextView should have PredefinedTextViewRoles.Structured view role");
                }
            }

            var vsUiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            return new RHistory(textView, _fileSystem, RToolsSettings.Current, _editorOperationsFactory, elisionBuffer, _rtfBuilderService, _textSearchService, vsUiShell);
        }
    }
}