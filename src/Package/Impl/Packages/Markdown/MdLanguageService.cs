// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Editors;
using Microsoft.VisualStudio.R.Package;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [Guid(MdGuidList.MdLanguageServiceGuidString)]
    internal sealed class MdLanguageService : BaseLanguageService {
        public MdLanguageService()
            : base(MdGuidList.MdLanguageServiceGuid,
                   MdContentTypeDefinition.LanguageName,
                   MdContentTypeDefinition.FileExtension) {
        }

        protected override string SaveAsFilter {
            get { return Resources.SaveAsFilterMD; }
        }
    }
}
