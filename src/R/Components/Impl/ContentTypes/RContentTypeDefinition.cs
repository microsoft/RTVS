// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.ContentTypes {
    /// <summary>
    /// Exports the R content type and file extension
    /// </summary>
    public sealed class RContentTypeDefinition {
        public const string LanguageName = "R";
        public const string ContentType = "R";
        public const string FileExtension = ".r";

        public const string RStudioProjectExtensionNoDot = "rproj";
        public const string VsRProjectExtension = ".rxproj";

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
