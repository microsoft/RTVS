// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Editors;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    [Guid(RGuidList.RLanguageServiceGuidString)]
    internal sealed class RLanguageService : BaseLanguageService {
        public RLanguageService()
            : base(RGuidList.RLanguageServiceGuid,
                   RContentTypeDefinition.LanguageName,
                   RContentTypeDefinition.FileExtension) {
        }

        protected override string SaveAsFilter {
            get { return Resources.SaveAsFilterR; }
        }

        public override int GetProximityExpressions(IVsTextBuffer pBuffer, int iLine, int iCol, int cLines, out IVsEnumBSTR ppEnum) {
            // TODO: implement this to light up Autos toolwindow.
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
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
