using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document
{
    /// <summary>
    /// Factory for Markdown language editor document
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    public class MdDocumentFactory : IEditorDocumentFactory
    {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance)
        {
            var document =  new MdEditorDocument(editorInstance.ViewBuffer, editorInstance.WorkspaceItem);
            return document;
        }
    }
}
