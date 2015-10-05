using System.Globalization;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Publishing
{
    internal sealed class MdPreviewCommand : PreviewCommandBase
    {
        public MdPreviewCommand(ITextView textView):
            base(textView)
        {
        }
        protected override IEditorDocument GetDocument(ITextBuffer textBuffer)
        {
            return MdEditorDocument.FromTextBuffer(TextView.TextBuffer);
        }

        protected override string RequiredPackageName
        {
            get { return "markdown"; }
        }

        protected override string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat)
        {
            string arguments = string.Format(CultureInfo.InvariantCulture,
               "\"markdown::renderMarkdown(\'{0}\', \'{1}\')\"", inputFile, outputFile);

            return arguments;
        }
    }
}
