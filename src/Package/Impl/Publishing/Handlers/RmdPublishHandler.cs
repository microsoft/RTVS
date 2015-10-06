using System.ComponentModel.Composition;
using System.Globalization;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;

namespace Microsoft.VisualStudio.R.Package.Publishing
{
    [Export(typeof(IMarkdownFlavorPublishHandler))]
    internal sealed class RmdPublishHandler : IMarkdownFlavorPublishHandler
    {
        public MarkdownFlavor Flavor
        {
            get { return MarkdownFlavor.R; }
        }

        public string RequiredPackageName
        {
            get { return "rmarkdown"; }
        }

        public string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat)
        {
            // Run rmarkdown::render
            return string.Format(CultureInfo.InvariantCulture, "\"rmarkdown::render(\'{0}\', \'html_document\')\"", inputFile, outputFile);
        }
    }
}
