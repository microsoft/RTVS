using System;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.History {
    internal class RHistoryProvider : IRHistoryProvider {
        private readonly IFileSystem _fileSystem;
        private readonly Lazy<IEditorOperationsFactoryService> _editorOperationsFactoryLazy;

        public RHistoryProvider(IFileSystem fileSystem, Lazy<IEditorOperationsFactoryService> editorOperationsFactoryLazy) {
            _fileSystem = fileSystem;
            _editorOperationsFactoryLazy = editorOperationsFactoryLazy;
        }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            return textView.Properties.GetOrCreateSingletonProperty(typeof(RHistory), () => new RHistory(textView, _fileSystem, _editorOperationsFactoryLazy.Value));
        }
    }
}