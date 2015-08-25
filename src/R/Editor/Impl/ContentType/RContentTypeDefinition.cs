using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.ContentType
{
    /// <summary>
    /// Exports the R content type and file extension
    /// </summary>
    public sealed class RContentTypeDefinition
    {
        public const string LanguageName = "R Language";
        public const string ContentType = "R Language";
        public const string FileExtension = ".r";
        public const string RStudioProjectExtension = "rproj";

        /// <summary>
        /// Exports the R content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(RContentTypeDefinition.ContentType)]
        [BaseDefinition("code")]
        public ContentTypeDefinition IRContentType { get; set; }

        /// <summary>
        /// Exports the R file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(RContentTypeDefinition.ContentType)]
        [FileExtension(RContentTypeDefinition.FileExtension)]
        public FileExtensionToContentTypeDefinition IRFileExtension { get; set; }
    }
}
