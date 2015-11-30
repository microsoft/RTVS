using System;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.History {
    internal class RHistoryProvider : IRHistoryProvider {
        private readonly IFileSystem _fileSystem;
        private readonly Lazy<IEditorOperationsFactoryService> _editorOperationsFactoryLazy;
        private readonly IRtfBuilderService _rtfBuilderService;

        public RHistoryProvider(IFileSystem fileSystem, Lazy<IEditorOperationsFactoryService> editorOperationsFactoryLazy, IRtfBuilderService rtfBuilderService) {
            _fileSystem = fileSystem;
            _editorOperationsFactoryLazy = editorOperationsFactoryLazy;
            _rtfBuilderService = rtfBuilderService;
        }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            return textView.Properties.GetOrCreateSingletonProperty(typeof(RHistory), () => CreateRHistory(textView));
        }

        private RHistory CreateRHistory(ITextView textView) {
            return new RHistory(textView, _fileSystem, _editorOperationsFactoryLazy.Value, _rtfBuilderService, AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell)));
        }
    }
}