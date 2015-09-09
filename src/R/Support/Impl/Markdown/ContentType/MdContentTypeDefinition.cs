using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Support.Markdown.ContentTypes
{
    public sealed class MdContentTypeDefinition
    {
        public const string LanguageName = "Markdown";
        public const string ContentType = "Markdown";
        public const string FileExtension1 = ".md";
        public const string FileExtension2 = ".markdown";

        /// <summary>
        /// Exports the MD content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(MdContentTypeDefinition.ContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IRdContentType { get; set; }

        /// <summary>
        /// Exports the markdown file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(MdContentTypeDefinition.ContentType)]
        [FileExtension(MdContentTypeDefinition.FileExtension1)]
        public FileExtensionToContentTypeDefinition IRdFileExtension1 { get; set; }

        /// <summary>
        /// Exports the markdown file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(MdContentTypeDefinition.ContentType)]
        [FileExtension(MdContentTypeDefinition.FileExtension2)]
        public FileExtensionToContentTypeDefinition IRdFileExtension2 { get; set; }
    }
}
