// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsTextLinesMock : VsTextBufferMock, IVsTextLines, IObjectWithSite, IConnectionPointContainer
    {
        public int AdviseTextLinesEvents(IVsTextLinesEvents pSink, out uint pdwCookie)
        {
            pdwCookie = 1;
            return VSConstants.S_OK;
        }

        public int CanReplaceLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, int iNewLen)
        {
            return VSConstants.S_OK;
        }

        public int CopyLineText(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszBuf, ref int pcchBuf)
        {
            return VSConstants.S_OK;
        }

        public int CreateEditPoint(int iLine, int iIndex, out object ppEditPoint)
        {
            ppEditPoint = null;
            return VSConstants.S_OK;
        }

        public int CreateLineMarker(int iMarkerType, int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IVsTextMarkerClient pClient, IVsTextLineMarker[] ppMarker)
        {
            return VSConstants.S_OK;
        }

        public int CreateTextPoint(int iLine, int iIndex, out object ppTextPoint)
        {
            ppTextPoint = null;
            return VSConstants.S_OK;
        }

        public int EnumMarkers(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, int iMarkerType, uint dwFlags, out IVsEnumLineMarkers ppEnum)
        {
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int FindMarkerByLineIndex(int iMarkerType, int iStartingLine, int iStartingIndex, uint dwFlags, out IVsTextLineMarker ppMarker)
        {
            ppMarker = null;
            return VSConstants.S_OK;
        }

        #region IObjectWithSite
        public void SetSite(object pUnkSite)
        {
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
