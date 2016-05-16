// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContentTypes {
    public sealed class MdProjectionContentTypeDefinition {
        public const string ContentType = "RmdProjection";

        /// <summary>
        /// Exports the projection R markdown content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(ContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IMdContentType { get; set; }
    }
}
