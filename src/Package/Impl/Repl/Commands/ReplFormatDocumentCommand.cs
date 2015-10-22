using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    class ReplFormatDocumentCommand : FormatDocumentCommand
    {
        public ReplFormatDocumentCommand(ITextView view, ITextBuffer buffer) : base(view, buffer)
        {
        }

        public override ITextBuffer TargetBuffer
        {
            get
            {
                return base.TargetBuffer.GetInteractiveWindow().CurrentLanguageBuffer;
            }
        }
    }
}
