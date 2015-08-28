using Microsoft.R.Core.AST;
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
        public AstRoot AstRoot { get; private set; }

        public RCompletionContext(ICompletionSession session, AstRoot ast, int position)
        {
            Session = session;
            Position = position;
            AstRoot = ast;
        }
    }
}
