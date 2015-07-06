using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller
{
    /// <summary>
    /// Command factory is exported via MEF for a given content
    /// type and allows adding commands to controllers
    /// via exports rather than directly in code.
    /// </summary>
    public interface ICommandFactory
    {
        IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer);
    }
}
