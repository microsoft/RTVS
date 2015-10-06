using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContentTypes
{
    public sealed class MdContentTypeDefinition
    {
        public const string LanguageName = "Markdown";
        public const string ContentType = "Markdown";
        public const string FileExtension1 = ".md";
        public const string FileExtension2 = ".markdown";
        public const string FileExtension3 = ".rmd";

        /// <summary>
        /// Exports the MD content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(MdContentTypeDefinition.ContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IMdContentType { get; set; }

        /// <summary>
        /// Exports the markdown file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(MdContentTypeDefinition.ContentType)]
        [FileExtension(MdContentTypeDefinition.FileExtension1)]
        public FileExtensionToContentTypeDefinition IMdFileExtension1 { get; set; }

        /// <summary>
        /// Exports the markdown file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(MdContentTypeDefinition.ContentType)]
        [FileExtension(MdContentTypeDefinition.FileExtension2)]
        public FileExtensionToContentTypeDefinition IMdFileExtension2 { get; set; }

        /// <summary>
        /// Exports the R markdown file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(MdContentTypeDefinition.ContentType)]
        [FileExtension(MdContentTypeDefinition.FileExtension3)]
        public FileExtensionToContentTypeDefinition IMdFileExtension3 { get; set; }
    }
}
