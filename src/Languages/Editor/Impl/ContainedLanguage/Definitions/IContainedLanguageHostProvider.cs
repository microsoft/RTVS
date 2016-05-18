using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Primary contained language host provider.
    /// Imported by the secondary language via MEF for 
    /// the primary language content type.
    /// </summary>
    public interface IContainedLanguageHostProvider {
        /// <summary>
        /// Retrieves contained language host for a given buffer
        /// </summary>
        /// <param name="textView">Primary text view</param>
        /// <param name="containedLanguageBuffer">Contained language text buffer</param>
        /// <returns>Contained language host</returns>
        IContainedLanguageHost GetContainedLanguageHost(ITextView textView, ITextBuffer containedLanguageBuffer);
    }
}
