using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Completions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands
{
    /// <summary>
    /// Completion controller in R code editor
    /// </summary>
    internal sealed class RCompletionCommandHandler : CompletionCommandHandler
    {
        public RCompletionCommandHandler(ITextView textView)
            : base(textView)
        {
        }

        public override CompletionController CompletionController
        {
            get { return ServiceManager.GetService<RCompletionController>(TextView); }
        }
    }
}
