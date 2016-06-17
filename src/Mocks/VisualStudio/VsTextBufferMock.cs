// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using MSXML;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public class VsTextBufferMock : IVsTextBuffer, IVsTextLines, IVsTextStream, IVsExpansion {
        private Guid _guidLangService = Guid.Empty;

        public ITextBuffer TextBuffer { get; internal set; }

        public VsTextBufferMock() :
            this(string.Empty) { }

        public VsTextBufferMock(string content) :
            this(content, "text") { }

        public VsTextBufferMock(IContentType contentType) :
            this(string.Empty, contentType.TypeName) { }

        public VsTextBufferMock(ITextBuffer buffer) {
            TextBuffer = buffer;
        }

        public VsTextBufferMock(string content, string contentType) {
            TextBuffer = new TextBufferMock(content, contentType);
        }

        #region IVsTextBuffer
        public int GetLanguageServiceID(out Guid pguidLangService) {
            pguidLangService = _guidLangService;
            return VSConstants.S_OK;
        }

        public int GetLastLineIndex(out int piLine, out int piIndex) {
            piLine = 0;
            piIndex = 0;
            return VSConstants.S_OK;
        }

        public int GetLengthOfLine(int iLine, out int piLength) {
            piLength = 0;
            return VSConstants.S_OK;
        }

        public int GetLineCount(out int piLineCount) {
            piLineCount = 0;
            return VSConstants.S_OK;
        }

        public int GetLineData(int iLine, LINEDATA[] pLineData, MARKERDATA[] pMarkerData) {
            return VSConstants.S_OK;
        }

        public int GetLineDataEx(uint dwFlags, int iLine, int iStartIndex, int iEndIndex, LINEDATAEX[] pLineData, MARKERDATA[] pMarkerData) {
            return VSConstants.S_OK;
        }

        public int GetLineIndexOfPosition(int iPosition, out int piLine, out int piColumn) {
            piLine = 0;
            piColumn = 0;
            return VSConstants.S_OK;
        }

        public int GetLineText(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, out string pbstrBuf) {
            pbstrBuf = null;
            return VSConstants.S_OK;
        }

        public int GetMarkerData(int iTopLine, int iBottomLine, MARKERDATA[] pMarkerData) {
            return VSConstants.S_OK;
        }

        public int GetPairExtents(TextSpan[] pSpanIn, TextSpan[] pSpanOut) {
            return VSConstants.S_OK;
        }

        public int GetPositionOfLine(int iLine, out int piPosition) {
            piPosition = 0;
            return VSConstants.S_OK;
        }

        public int GetPositionOfLineIndex(int iLine, int iIndex, out int piPosition) {
            piPosition = 0;
            return VSConstants.S_OK;
        }

        public void GetSite(ref Guid riid, out IntPtr ppvSite) {
            throw new NotImplementedException();
        }

        public int GetSize(out int piLength) {
            piLength = 0;
            return VSConstants.S_OK;
        }

        public int GetStateFlags(out uint pdwReadOnlyFlags) {
            pdwReadOnlyFlags = 0;
            return VSConstants.S_OK;
        }

        public int GetUndoManager(out IOleUndoManager ppUndoManager) {
            ppUndoManager = null;
            return VSConstants.S_OK;
        }

        public int InitializeContent(string pszText, int iLength) {
            return VSConstants.S_OK;
        }

        public int IVsTextLinesReserved1(int iLine, LINEDATA[] pLineData, int fAttributes) {
            return VSConstants.S_FALSE;
        }

        public int LockBuffer() {
            return VSConstants.S_OK;
        }

        public int LockBufferEx(uint dwFlags) {
            return VSConstants.S_OK;
        }

        public int ReleaseLineData(LINEDATA[] pLineData) {
            return VSConstants.S_OK;
        }

        public int ReleaseLineDataEx(LINEDATAEX[] pLineData) {
            return VSConstants.S_OK;
        }

        public int ReleaseMarkerData(MARKERDATA[] pMarkerData) {
            return VSConstants.S_OK;
        }

        public int Reload(int fUndoable) {
            return VSConstants.S_OK;
        }

        public int ReloadLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan) {
            return VSConstants.S_OK;
        }

        public int ReplaceLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan) {
            return VSConstants.S_OK;
        }

        public int ReplaceLinesEx(uint dwFlags, int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan) {
            return VSConstants.S_OK;
        }

        public int Reserved1() {
            return VSConstants.S_OK;
        }

        public int Reserved10() {
            return VSConstants.S_OK;
        }

        public int Reserved2() {
            return VSConstants.S_OK;
        }

        public int Reserved3() {
            return VSConstants.S_OK;
        }

        public int Reserved4() {
            return VSConstants.S_OK;
        }

        public int Reserved5() {
            return VSConstants.S_OK;
        }

        public int Reserved6() {
            return VSConstants.S_OK;
        }

        public int Reserved7() {
            return VSConstants.S_OK;
        }

        public int Reserved8() {
            return VSConstants.S_OK;
        }

        public int Reserved9() {
            return VSConstants.S_OK;
        }

        public int SetLanguageServiceID(ref Guid guidLangService) {
            _guidLangService = guidLangService;
            return VSConstants.S_OK;
        }

        public int UnadviseTextLinesEvents(uint dwCookie) {
            return VSConstants.S_OK;
        }

        public int UnlockBuffer() {
            return VSConstants.S_OK;
        }

        public int UnlockBufferEx(uint dwFlags) {
            return VSConstants.S_OK;
        }

        public int SetStateFlags(uint dwReadOnlyFlags) {
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsExpansion
        public int InsertExpansion(TextSpan tsContext, TextSpan tsInsertPos, IVsExpansionClient pExpansionClient, Guid guidLang, out IVsExpansionSession pSession) {
            TextBuffer.Insert(0, "expansion");
            pSession = new VsExpansionSessionMock();
            return VSConstants.S_OK;
        }

        public int InsertNamedExpansion(string bstrTitle, string bstrPath, TextSpan tsInsertPos, IVsExpansionClient pExpansionClient, Guid guidLang, int fShowDisambiguationUI, out IVsExpansionSession pSession) {
            TextBuffer.Insert(0, bstrTitle);
            pSession = new VsExpansionSessionMock();
            return VSConstants.S_OK;
        }

        public int InsertSpecificExpansion(IXMLDOMNode pSnippet, TextSpan tsInsertPos, IVsExpansionClient pExpansionClient, Guid guidLang, string pszRelativePath, out IVsExpansionSession pSession) {
            TextBuffer.Insert(0, "specific-expansion");
            pSession = new VsExpansionSessionMock();
            return VSConstants.S_OK;
        }

        public int CopyLineText(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszBuf, ref int pcchBuf) {
            throw new NotImplementedException();
        }

        public int CanReplaceLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, int iNewLen) {
            throw new NotImplementedException();
        }

        public int CreateLineMarker(int iMarkerType, int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IVsTextMarkerClient pClient, IVsTextLineMarker[] ppMarker) {
            throw new NotImplementedException();
        }

        public int EnumMarkers(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, int iMarkerType, uint dwFlags, out IVsEnumLineMarkers ppEnum) {
            throw new NotImplementedException();
        }

        public int FindMarkerByLineIndex(int iMarkerType, int iStartingLine, int iStartingIndex, uint dwFlags, out IVsTextLineMarker ppMarker) {
            throw new NotImplementedException();
        }

        public int AdviseTextLinesEvents(IVsTextLinesEvents pSink, out uint pdwCookie) {
            throw new NotImplementedException();
        }

        public int CreateEditPoint(int iLine, int iIndex, out object ppEditPoint) {
            throw new NotImplementedException();
        }

        public int CreateTextPoint(int iLine, int iIndex, out object ppTextPoint) {
            throw new NotImplementedException();
        }

        public int GetStream(int iPos, int iLength, IntPtr pszDest) {
            throw new NotImplementedException();
        }

        public int ReplaceStream(int iPos, int iOldLen, IntPtr pszText, int iNewLen) {
            if (pszText != IntPtr.Zero) {
                var str = Marshal.PtrToStringBSTR(pszText);
                TextBuffer.Replace(new Span(iPos, iOldLen), str);
            } else {
                TextBuffer.Delete(new Span(iPos, iOldLen));
            }
            return VSConstants.S_OK;
        }

        public int CanReplaceStream(int iPos, int iOldLen, int iNewLen) {
            return VSConstants.S_OK;
        }

        public int CreateStreamMarker(int iMarkerType, int iPos, int iLength, IVsTextMarkerClient pClient, IVsTextStreamMarker[] ppMarker) {
            throw new NotImplementedException();
        }

        public int EnumMarkers(int iPos, int iLen, int iMarkerType, uint dwFlags, out IVsEnumStreamMarkers ppEnum) {
            ppEnum = new VsEnumStreamMarkersMock(new IVsTextStreamMarker[1] { new VsTextStreamMarker(this, iPos, 1) });
            return VSConstants.S_OK;
        }

        public int FindMarkerByPosition(int iMarkerType, int iStartingPos, uint dwFlags, out IVsTextStreamMarker ppMarker) {
            throw new NotImplementedException();
        }

        public int AdviseTextStreamEvents(IVsTextStreamEvents pSink, out uint pdwCookie) {
            throw new NotImplementedException();
        }

        public int UnadviseTextStreamEvents(uint dwCookie) {
            throw new NotImplementedException();
        }

        public int ReloadStream(int iPos, int iOldLen, IntPtr pszText, int iNewLen) {
            throw new NotImplementedException();
        }

        public int CreateEditPoint(int iPosition, out object ppEditPoint) {
            throw new NotImplementedException();
        }

        public int ReplaceStreamEx(uint dwFlags, int iPos, int iOldLen, IntPtr pszText, int iNewLen, out int piActualLen) {
            throw new NotImplementedException();
        }

        public int CreateTextPoint(int iPosition, out object ppTextPoint) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
