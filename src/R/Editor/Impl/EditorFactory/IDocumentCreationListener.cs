using Microsoft.Languages.Editor.EditorFactory;

namespace Microsoft.Html.Editor.EditorFactory
{
    /// <summary>
    /// Exported via MEF. When document is created
    /// every exported listener will be called.
    /// </summary>
    public interface IDocumentCreationListener
    {
        void DocumentCreated(IEditorDocument document);
    }
}
