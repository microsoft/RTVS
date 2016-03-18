// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsExpansionManagerMock : IVsExpansionManager {
        public int EnumerateExpansions(Guid guidLang, int fShortCutOnly, string[] bstrTypes, int iCountTypes, int fIncludeNULLType, int fIncludeDuplicates, out IVsExpansionEnumeration pEnum) {
            pEnum = new VsExpansionEnumerationMock();
            return VSConstants.S_OK;
        }

        public int GetExpansionByShortcut(IVsExpansionClient pClient, Guid guidLang, string szShortcut, IVsTextView pView, TextSpan[] pts, int fShowUI, out string pszExpansionPath, out string pszTitle) {
            throw new NotImplementedException();
        }

        public int GetSnippetShortCutKeybindingState(out int fBound) {
            throw new NotImplementedException();
        }

        public int GetTokenPath(uint token, out string pbstrPath) {
            throw new NotImplementedException();
        }

        public int InvokeInsertionUI(IVsTextView pView, IVsExpansionClient pClient, Guid guidLang, string[] bstrTypes, int iCountTypes, int fIncludeNULLType, string[] bstrKinds, int iCountKinds, int fIncludeNULLKind, string bstrPrefixText, string bstrCompletionChar) {
            return VSConstants.S_OK;
        }
    }
}
