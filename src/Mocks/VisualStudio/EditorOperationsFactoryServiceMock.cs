using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IEditorOperationsFactoryService))]
    public sealed class EditorOperationsFactoryServiceMock : IEditorOperationsFactoryService {
        public IEditorOperations GetEditorOperations(ITextView textView) {
            return new EditorOperationsMock(textView);
        }
    }
}
