// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History {
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