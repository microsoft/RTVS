using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContentTypes
{
    public sealed class RmdContentTypeDefinition
    {
        public const string LanguageName = "R Markdown";
        public const string ContentType = "RMarkdown";
        public const string FileExtension = ".rmd";

        /// <summary>
        /// Exports the RMD content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(RmdContentTypeDefinition.ContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IRmdContentType { get; set; }

        /// <summary>
        /// Exports the R markdown file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(RmdContentTypeDefinition.ContentType)]
        [FileExtension(RmdContentTypeDefinition.FileExtension)]
        public FileExtensionToContentTypeDefinition IRmdFileExtension { get; set; }
    }
}
