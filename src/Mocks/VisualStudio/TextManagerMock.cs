// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TextManagerMock : IConnectionPointContainer, IVsTextManager3, IVsTextManager4 // IVsTextManager, IVsTextManager2
    {
        #region IVsTextManager3
        public int FindLanguageSIDForExtensionlessFilename(string pszFileName, out Guid pguidLangSID)
        {
            pguidLangSID = Guid.Empty;
            return VSConstants.S_OK;
        }

        public int GetUserPreferences3(VIEWPREFERENCES3[] pViewPrefs, FRAMEPREFERENCES2[] pFramePrefs, LANGPREFERENCES2[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs)
        {
            return VSConstants.S_OK;
        }

        public int PrimeExpansionManager(ref Guid guidLang)
        {
            return VSConstants.S_OK;
        }

        public int SetUserPreferences3(VIEWPREFERENCES3[] pViewPrefs, FRAMEPREFERENCES2[] pFramePrefs, LANGPREFERENCES2[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsTextManager4
        public int GetUserPreferences4(VIEWPREFERENCES3[] pViewPrefs, LANGPREFERENCES3[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs)
        {
            return VSConstants.S_OK;
        }

        public int SetUserPreferences4(VIEWPREFERENCES3[] pViewPrefs, LANGPREFERENCES3[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region IConnectionPointContainer
        public void EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
        {
            ppEnum = null;
        }

        public void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
        {
            ppCP = new ConnectionPointMock(this);
        }
        #endregion
    }
}
