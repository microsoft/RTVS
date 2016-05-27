// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Languages;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [Guid(MdGuidList.MdLanguageServiceGuidString)]
    internal sealed class MdLanguageService : BaseLanguageService {
        public MdLanguageService()
            : base(MdGuidList.MdLanguageServiceGuid,
                   MdContentTypeDefinition.LanguageName,
                   MdContentTypeDefinition.FileExtension1 + ";" +
                   MdContentTypeDefinition.FileExtension2 + ";" +
                   MdContentTypeDefinition.FileExtension3) {
        }

        protected override string SaveAsFilter {
            get { return Resources.SaveAsFilterMD; }
        }

        public override int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol, TextSpan[] pCodeSpan) {
            pCodeSpan[0] = default(TextSpan);
            pCodeSpan[0].iStartLine = iLine;
            pCodeSpan[0].iEndLine = iLine;

            // Returning E_NOTIMPL indicates that this language only supports entire-line breakpoints. Consequently,
            // VS debugger will highlight the entire line when the breakpoint is active and the corresponding option
            // ("Highlight entire source line for breakpoints and current statement") is set. If we returned S_OK,
            // we'd have to handle this ourselves.
            return VSConstants.E_NOTIMPL;
        }
    }
}
