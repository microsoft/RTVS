using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    internal sealed class ReplTypingCommandHandler: RTypingCommandHandler
    {
        public ReplTypingCommandHandler(ITextView textView) :
            base(textView)
        {
        }

        protected override void HandleCompletion(char typedChar)
        {
            if(typedChar == '{')
            {
                return;
            }

            base.HandleCompletion(typedChar);
        }
    }
}
