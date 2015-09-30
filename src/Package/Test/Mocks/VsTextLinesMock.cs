using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks
{
    public sealed class VsTextLinesMock : IVsTextLines
    {
        private Guid _guidLangService = Guid.Empty;

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

        public int GetLanguageServiceID(out Guid pguidLangService)
        {
            pguidLangService = _guidLangService;
            return VSConstants.S_OK;
        }

        public int GetLastLineIndex(out int piLine, out int piIndex)
        {
            piLine = 0;
            piIndex = 0;
            return VSConstants.S_OK;
        }

        public int GetLengthOfLine(int iLine, out int piLength)
        {
            piLength = 0;
            return VSConstants.S_OK;
        }

        public int GetLineCount(out int piLineCount)
        {
            piLineCount = 0;
            return VSConstants.S_OK;
        }

        public int GetLineData(int iLine, LINEDATA[] pLineData, MARKERDATA[] pMarkerData)
        {
            return VSConstants.S_OK;
        }

        public int GetLineDataEx(uint dwFlags, int iLine, int iStartIndex, int iEndIndex, LINEDATAEX[] pLineData, MARKERDATA[] pMarkerData)
        {
            return VSConstants.S_OK;
        }

        public int GetLineIndexOfPosition(int iPosition, out int piLine, out int piColumn)
        {
            piLine = 0;
            piColumn = 0;
            return VSConstants.S_OK;
        }

        public int GetLineText(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, out string pbstrBuf)
        {
            pbstrBuf = null;
            return VSConstants.S_OK;
        }

        public int GetMarkerData(int iTopLine, int iBottomLine, MARKERDATA[] pMarkerData)
        {
            return VSConstants.S_OK;
        }

        public int GetPairExtents(TextSpan[] pSpanIn, TextSpan[] pSpanOut)
        {
            return VSConstants.S_OK;
        }

        public int GetPositionOfLine(int iLine, out int piPosition)
        {
            piPosition = 0;
            return VSConstants.S_OK;
        }

        public int GetPositionOfLineIndex(int iLine, int iIndex, out int piPosition)
        {
            piPosition = 0;
            return VSConstants.S_OK;
        }

        public int GetSize(out int piLength)
        {
            piLength = 0;
            return VSConstants.S_OK;
        }

        public int GetStateFlags(out uint pdwReadOnlyFlags)
        {
            pdwReadOnlyFlags = 0;
            return VSConstants.S_OK;
        }

        public int GetUndoManager(out IOleUndoManager ppUndoManager)
        {
            ppUndoManager = null;
            return VSConstants.S_OK;
        }

        public int InitializeContent(string pszText, int iLength)
        {
            return VSConstants.S_OK;
        }

        public int IVsTextLinesReserved1(int iLine, LINEDATA[] pLineData, int fAttributes)
        {
            return VSConstants.S_FALSE;
        }

        public int LockBuffer()
        {
            return VSConstants.S_OK;
        }

        public int LockBufferEx(uint dwFlags)
        {
            return VSConstants.S_OK;
        }

        public int ReleaseLineData(LINEDATA[] pLineData)
        {
            return VSConstants.S_OK;
        }

        public int ReleaseLineDataEx(LINEDATAEX[] pLineData)
        {
            return VSConstants.S_OK;
        }

        public int ReleaseMarkerData(MARKERDATA[] pMarkerData)
        {
            return VSConstants.S_OK;
        }

        public int Reload(int fUndoable)
        {
            return VSConstants.S_OK;
        }

        public int ReloadLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan)
        {
            return VSConstants.S_OK;
        }

        public int ReplaceLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan)
        {
            return VSConstants.S_OK;
        }

        public int ReplaceLinesEx(uint dwFlags, int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan)
        {
            return VSConstants.S_OK;
        }

        public int Reserved1()
        {
            return VSConstants.S_OK;
        }

        public int Reserved10()
        {
            return VSConstants.S_OK;
        }

        public int Reserved2()
        {
            return VSConstants.S_OK;
        }

        public int Reserved3()
        {
            return VSConstants.S_OK;
        }

        public int Reserved4()
        {
            return VSConstants.S_OK;
        }

        public int Reserved5()
        {
            return VSConstants.S_OK;
        }

        public int Reserved6()
        {
            return VSConstants.S_OK;
        }

        public int Reserved7()
        {
            return VSConstants.S_OK;
        }

        public int Reserved8()
        {
            return VSConstants.S_OK;
        }

        public int Reserved9()
        {
            return VSConstants.S_OK;
        }

        public int SetLanguageServiceID(ref Guid guidLangService)
        {
            _guidLangService = guidLangService;
            return VSConstants.S_OK;
        }

        public int SetStateFlags(uint dwReadOnlyFlags)
        {
            return VSConstants.S_OK;
        }

        public int UnadviseTextLinesEvents(uint dwCookie)
        {
            return VSConstants.S_OK;
        }

        public int UnlockBuffer()
        {
            return VSConstants.S_OK;
        }

        public int UnlockBufferEx(uint dwFlags)
        {
            return VSConstants.S_OK;
        }
    }
}
