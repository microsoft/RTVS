
namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// Document factory 
    /// </summary>
    public interface IEditorDocumentFactory {
        /// <summary>
        /// Creates instance if editor document
        /// </summary>
        IEditorDocument CreateDocument(IEditorInstance editorInstance);
    }
}
