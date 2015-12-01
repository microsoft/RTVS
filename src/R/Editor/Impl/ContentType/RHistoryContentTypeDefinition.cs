using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.ContentType {
    /// <summary>
    /// Exports the R history content type and file extension
    /// </summary>
    public sealed class RHistoryContentTypeDefinition
    {
        public const string ContentType = "RHistory";
        public const string FileExtension = ".rhistory";

        /// <summary>
        /// Exports the R content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(ContentType)]
        [BaseDefinition("code")]
        public ContentTypeDefinition IRContentType { get; set; }

        /// <summary>
        /// Exports the R file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(ContentType)]
        [FileExtension(FileExtension)]
        public FileExtensionToContentTypeDefinition IRFileExtension { get; set; }
    }
}