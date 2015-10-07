using System.ComponentModel.Composition;
using System.Globalization;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;

namespace Microsoft.VisualStudio.R.Package.Publishing
{
    [Export(typeof(IMarkdownFlavorPublishHandler))]
    internal sealed class MdPublishHandler : IMarkdownFlavorPublishHandler
    {
        public MarkdownFlavor Flavor
        {
            get { return MarkdownFlavor.Basic; }
        }

        public string RequiredPackageName
        {
            get { return "markdown"; }
        }

        public string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat)
        {
            string arguments = string.Format(CultureInfo.InvariantCulture,
               "\"markdown::renderMarkdown(\'{0}\', \'{1}\')\"", inputFile, outputFile);

            return arguments;
        }
    }
}
