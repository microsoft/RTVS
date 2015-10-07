using Microsoft.Markdown.Editor.Flavor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Definitions
{
    public interface IMarkdownFlavorPublishHandler
    {
        MarkdownFlavor Flavor { get; }

        string RequiredPackageName { get; }
       
        string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat);
    }
}
