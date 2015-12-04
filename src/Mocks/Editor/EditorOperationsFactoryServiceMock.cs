using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.Editor.Mocks {
    public sealed class EditorOperationsFactoryServiceMock : IEditorOperationsFactoryService {
        public IEditorOperations GetEditorOperations(ITextView textView) {
            return new EditorOperationsMock(textView);
        }
    }
}
