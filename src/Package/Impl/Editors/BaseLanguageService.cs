// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Editors {
    /// <summary>
    /// Basic language service implementation for Visual Studio.
    /// Mostly provides interfaces that return 'not implemented'
    /// since VS architecture changed since and colorization, 
    /// formatting and similar operations are implemented in objects
    /// exported via MEF and imported by the core editor directly.
    /// </summary>
    internal class BaseLanguageService : IVsLanguageInfo, IVsLanguageTextOps, IVsLanguageDebugInfo, IVsFormatFilterProvider {
        private string _languageName;
        private string _fileExtensions;
        private Guid _languageServiceGuid;

        public BaseLanguageService(Guid languageServiceId, string languageName, string fileExtensions) {
            _languageServiceGuid = languageServiceId;
            _languageName = languageName;
            _fileExtensions = fileExtensions;
        }

        #region IVsLanguageInfo
        public virtual int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr) {
            ppCodeWinMgr = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer) {
            ppColorizer = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetFileExtensions(out string pbstrExtensions) {
            pbstrExtensions = _fileExtensions;
            return VSConstants.S_OK;
        }

        public int GetLanguageName(out string bstrName) {
            bstrName = _languageName;
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsLanguageTextOps
        public virtual int Format(IVsTextLayer textLayer, TextSpan[] span) => VSConstants.E_NOTIMPL;
        public virtual int GetDataTip(IVsTextLayer textLayer, TextSpan[] span, TextSpan[] tipSpan, out string text) {
            text = null;
            return VSConstants.E_NOTIMPL;
        }
        public virtual int GetPairExtent(IVsTextLayer textLayer, TextAddress textAddress, TextSpan[] span) => VSConstants.E_NOTIMPL;
        public virtual int GetWordExtent(IVsTextLayer textLayer, TextAddress textAddress, WORDEXTFLAGS flags, TextSpan[] span) => VSConstants.E_NOTIMPL;
        #endregion

        #region IVsLanguageDebugInfo
        public virtual int GetLanguageID(IVsTextBuffer pBuffer, int iLine, int iCol, out Guid pguidLanguageID) {
            pguidLanguageID = _languageServiceGuid;
            return VSConstants.S_OK;
        }

        public virtual int GetLocationOfName(string pszName, out string pbstrMkDoc, TextSpan[] pspanLocation) {
            pbstrMkDoc = null;
            return VSConstants.E_NOTIMPL;
        }

        public virtual int GetNameOfLocation(IVsTextBuffer pBuffer, int iLine, int iCol, out string pbstrName, out int piLineOffset) {
            pbstrName = null;
            piLineOffset = 0;
            return VSConstants.E_NOTIMPL;
        }

        public virtual int GetProximityExpressions(IVsTextBuffer pBuffer, int iLine, int iCol, int cLines, out IVsEnumBSTR ppEnum) {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public virtual int IsMappedLocation(IVsTextBuffer pBuffer, int iLine, int iCol) {
            return VSConstants.S_FALSE;
        }

        public virtual int ResolveName(string pszName, uint dwFlags, out IVsEnumDebugName ppNames) {
            ppNames = null;
            return VSConstants.E_NOTIMPL;
        }

        public virtual int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol, TextSpan[] pCodeSpan) {
            return VSConstants.S_FALSE;
        }
        #endregion

        public int CurFileExtensionFormat(string fileName, out uint extensionIndex) {
            extensionIndex = 0;
            string desiredFileext = "*" + Path.GetExtension(fileName);

            var filters = SaveAsFilter.Split('|');
            for (uint i = 0; i < filters.Length - 1; i += 2) {
                var filterExtensions = filters[i + 1].Split(';');
                if (filterExtensions.Contains(desiredFileext, StringComparer.OrdinalIgnoreCase)) {
                    extensionIndex = i / 2;
                }
            }
            return VSConstants.S_OK;
        }

        public int GetFormatFilterList(out string filterList) {
            filterList = SaveAsFilter.Replace('|', '\n');
            return VSConstants.S_OK;
        }

        public int QueryInvalidEncoding(uint Format, out string message) {
            message = null;
            return VSConstants.S_FALSE;
        }

        protected virtual string SaveAsFilter => string.Empty;
    }
}
