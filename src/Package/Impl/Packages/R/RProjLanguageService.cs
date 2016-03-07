// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Languages;

namespace Microsoft.VisualStudio.R.Packages.R {
    /// <summary>
    /// Fake language service to make VS open rproj files in the editor factory 
    /// that instead of opening .rproj file in the editor locates matching 
    /// .rxproj file, if any, and opens the project instead.
    /// </summary>
    [Guid(RGuidList.RProjLanguageServiceGuidString)]
    internal sealed class RProjLanguageService : BaseLanguageService {
        public RProjLanguageService()
            : base(RGuidList.RProjLanguageServiceGuid,
                   RContentTypeDefinition.RProjectName,
                   RContentTypeDefinition.RStudioProjectExtension) {
        }
    }
}
