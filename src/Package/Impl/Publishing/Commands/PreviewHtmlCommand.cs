using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands {
    internal sealed class PreviewHtmlCommand : PreviewCommand {
        public PreviewHtmlCommand(ITextView textView)
            : base(textView, (int)MdPackageCommandId.icmdPreviewHtml) {
        }

        protected override string FileExtension {
            get { return "html"; }
        }

        protected override PublishFormat Format {
            get { return PublishFormat.Html; }
        }
    }
}
