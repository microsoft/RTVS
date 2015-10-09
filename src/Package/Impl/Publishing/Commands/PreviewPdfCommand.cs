using System;
using Microsoft.Languages.Editor;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Commands
{
    internal sealed class PreviewPdfCommand : PreviewCommand
    {
        public PreviewPdfCommand(ITextView textView)
            : base(textView, (int)MdPackageCommandId.icmdPreviewPdf)
        {
        }

        protected override string FileExtension
        {
            get { return "pdf"; }
        }

        protected override PublishFormat Format
        {
            get { return PublishFormat.Pdf; }
        }
    }
}
