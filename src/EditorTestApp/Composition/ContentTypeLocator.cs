using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Composition
{
    internal class ContentTypeLocator
    {
        [Import]
        IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        IFileExtensionRegistryService FileExtensionRegistryService { get; set; }

        public ContentTypeLocator(ICompositionService cs)
        {
            cs.SatisfyImportsOnce(this);
        }

        public IContentType FindContentType(string filePath)
        {
            return FileExtensionRegistryService.GetContentTypeForExtension(Path.GetExtension(filePath));
        }
    }
}
