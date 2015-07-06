using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Factory for R language editor document
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    public class RDocumentFactory : IEditorDocumentFactory
    {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance)
        {
            var document =  new REditorDocument(editorInstance.ViewBuffer, editorInstance.WorkspaceItem);
            return document;
        }
    }
}
