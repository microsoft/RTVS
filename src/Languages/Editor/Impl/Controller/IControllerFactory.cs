using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller {
    /// <summary>
    /// Controller factory is exported via MEF for a given content
    /// type and allows adding controllers chained below the main controller
    /// via exports rather than directly in code.
    /// </summary>
    public interface IControllerFactory {
        IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer);
    }
}