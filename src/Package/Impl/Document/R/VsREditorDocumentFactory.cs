using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;

namespace Microsoft.VisualStudio.R.Package.Document.R {
    [Export(typeof(IVsEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class VsREditorDocumentFactory : IVsEditorDocumentFactory {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new VsREditorDocument(editorInstance);
        }
    }
}
