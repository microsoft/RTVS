using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Factory for R language editor document
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    public class DocumentFactory : IEditorDocumentFactory
    {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance)
        {
            var document =  new EditorDocument(editorInstance.ViewBuffer, editorInstance.WorkspaceItem);
            return document;
        }
    }
}
