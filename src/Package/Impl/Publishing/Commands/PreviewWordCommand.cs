using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal sealed class PreviewWordCommand : PreviewCommand {
        public PreviewWordCommand(ITextView textView)
            : base(textView, (int)MdPackageCommandId.icmdPreviewWord) {
        }

        protected override string FileExtension {
            get { return "docx"; }
        }

        protected override PublishFormat Format {
            get { return PublishFormat.Word; }
        }
    }
}
