// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.RData.ContentTypes {
    public sealed class RdContentTypeDefinition {
        public const string LanguageName = "RDoc";
        public const string ContentType = "RDoc";
        public const string FileExtension = ".rd";

        /// <summary>
        /// Exports the RD content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(RdContentTypeDefinition.ContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IRdContentType { get; set; }

        /// <summary>
        /// Exports the R file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(RdContentTypeDefinition.ContentType)]
        [FileExtension(RdContentTypeDefinition.FileExtension)]
        public FileExtensionToContentTypeDefinition IRdFileExtension { get; set; }
    }
}
