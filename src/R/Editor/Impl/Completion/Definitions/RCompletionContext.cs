using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Definitions
{
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public class RCompletionContext
    {
        public int Position { get; set; }
        public ICompletionSession Session { get; private set; }

        public RCompletionContext(ICompletionSession session, int position)
        {
            this.Session = session;
            this.Position = position;
        }

        public RCompletionContext()
        {
        }
    }
}
