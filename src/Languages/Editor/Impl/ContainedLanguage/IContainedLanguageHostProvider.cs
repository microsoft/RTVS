using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Primary contained language host provider. Provided as a services 
    /// to the editor for the primary language content type.
    /// </summary>
    public interface IContainedLanguageHostProvider {
        /// <summary>
        /// Retrieves contained language host for a given buffer
        /// </summary>
        /// <param name="editorView">Primary text view</param>
        /// <param name="containedLanguageBuffer">Contained language text buffer</param>
        /// <returns>Contained language host</returns>
        IContainedLanguageHost GetContainedLanguageHost(IEditorView editorView, IEditorBuffer containedLanguageBuffer);
    }
}
