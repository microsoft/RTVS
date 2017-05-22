// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContentTypes {
    public sealed class MdContentTypeDefinition {
        public const string LanguageName = "R Markdown";
        public const string ContentType = "R Markdown";
        public const string FileExtension = ".rmd";

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
        [FileExtension(MdContentTypeDefinition.FileExtension)]
        public FileExtensionToContentTypeDefinition IMdFileExtension { get; set; }
    }
}
