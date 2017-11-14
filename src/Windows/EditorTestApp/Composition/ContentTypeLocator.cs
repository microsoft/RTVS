// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Composition
{
    [ExcludeFromCodeCoverage]
    internal sealed class ContentTypeLocator
    {
        [Import]
        IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        IFileExtensionRegistryService FileExtensionRegistryService { get; set; }

        public ContentTypeLocator(ICompositionService cs)
        {
            cs.SatisfyImportsOnce(this);
        }

        public IContentType FindContentType(string filePath)
        {
            return FileExtensionRegistryService.GetContentTypeForExtension(Path.GetExtension(filePath));
        }
    }
}
