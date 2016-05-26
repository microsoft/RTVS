// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContentTypes {
    public sealed class MdProjectionContentTypeDefinition {
        public const string ContentType = "RmdProjection";

        /// <summary>
        /// Exports the MD content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(MdProjectionContentTypeDefinition.ContentType)]
        [BaseDefinition("projection")]
        public ContentTypeDefinition IMdContentType { get; set; }
    }
}
