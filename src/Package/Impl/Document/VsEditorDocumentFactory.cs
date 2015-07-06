using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.ContentType;

namespace Microsoft.VisualStudio.R.Package.Document
{
    [Export(typeof(IVsEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class VsEditorDocumentFactory : IVsEditorDocumentFactory
    {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance)
        {
            return new VsREditorDocument(editorInstance);
        }
    }
}
