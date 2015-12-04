using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [Export(typeof(IEditorOperationsFactoryService))]
    public sealed class EditorOperationsFactoryServiceMock : IEditorOperationsFactoryService {
        public IEditorOperations GetEditorOperations(ITextView textView) {
            throw new NotImplementedException();
        }
    }
}
