using System.Globalization;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing
{
    internal sealed class RmdPreviewCommand : PreviewCommandBase
    {
        public RmdPreviewCommand(ITextView textView):
            base(textView)
        {
        }
        protected override IEditorDocument GetDocument(ITextBuffer textBuffer)
        {
            return MdEditorDocument.FromTextBuffer(TextView.TextBuffer);
        }

        protected override string RequiredPackageName
        {
            get { return "rmarkdown"; }
        }

        protected override string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat)
        {
            // Run rmarkdown::render
            return string.Format(CultureInfo.InvariantCulture, "\"rmarkdown::render(\'{0}\')\"", inputFile, outputFile);
        }
    }
}
