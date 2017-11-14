// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class VsTextViewMock : IVsTextView
    {
        private IVsTextLines _buffer;
        private TextSelMode _selMode = TextSelMode.SM_STREAM;
        private int _iAnchorLine, _iAnchorCol, _iEndLine, _iEndCol;
        private int _caretLine, _caretColumn;

        public ITextView TextView { get; private set; }

        public VsTextViewMock()
        {
            TextView = new TextViewMock(new TextBufferMock(string.Empty, "text"));
        }

        public VsTextViewMock(ITextView textView)
        {
            TextView = textView;
        }

        public int AddCommandFilter(IOleCommandTarget pNewCmdTarg, out IOleCommandTarget ppNextCmdTarg)
        {
            ppNextCmdTarg = null;
            return VSConstants.S_OK;
        }

        public int CenterColumns(int iLine, int iLeftCol, int iColCount)
        {
            return VSConstants.S_OK;
        }

        public int CenterLines(int iTopLine, int iCount)
        {
            return VSConstants.S_OK;
        }

        public int ClearSelection(int fMoveToAnchor)
        {
            return VSConstants.S_OK;
        }

        public int CloseView()
        {
            return VSConstants.S_OK;
        }

        public int EnsureSpanVisible(TextSpan span)
        {
            return VSConstants.S_OK;
        }

        public int GetBuffer(out IVsTextLines ppBuffer)
        {
            ppBuffer = _buffer;
            return VSConstants.S_OK;
        }

        public int GetCaretPos(out int piLine, out int piColumn)
        {
            piLine = _caretLine;
            piColumn = _caretColumn;
            return VSConstants.S_OK;
        }

        public int GetLineAndColumn(int iPos, out int piLine, out int piIndex)
        {
            piLine = 0;
            piIndex = 0;
            return VSConstants.S_OK;
        }

        public int GetLineHeight(out int piLineHeight)
        {
            piLineHeight = 0;
            return VSConstants.S_OK;
        }

        public int GetNearestPosition(int iLine, int iCol, out int piPos, out int piVirtualSpaces)
        {
            piPos = 0;
            piVirtualSpaces = 0;
            return VSConstants.S_OK;
        }

        public int GetPointOfLineColumn(int iLine, int iCol, POINT[] ppt)
        {
            return VSConstants.S_OK;
        }

        public int GetScrollInfo(int iBar, out int piMinUnit, out int piMaxUnit, out int piVisibleUnits, out int piFirstVisibleUnit)
        {
            piMinUnit = 0;
            piMaxUnit = 0;
            piFirstVisibleUnit = 0;
            piVisibleUnits = 0;
            return VSConstants.S_OK;
        }

        public int GetSelectedText(out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        public int GetSelection(out int piAnchorLine, out int piAnchorCol, out int piEndLine, out int piEndCol)
        {
            piAnchorLine = _iAnchorLine;
            piAnchorCol = _iAnchorCol;
            piEndLine = _iEndLine;
            piEndCol = _iEndCol;
            return VSConstants.S_OK;
        }

        public int GetSelectionDataObject(out IDataObject ppIDataObject)
        {
            ppIDataObject = null;
            return VSConstants.S_OK;
        }

        public TextSelMode GetSelectionMode()
        {
            return _selMode;
        }

        public int GetSelectionSpan(TextSpan[] pSpan)
        {
            return VSConstants.S_OK;
        }

        public int GetTextStream(int iTopLine, int iTopCol, int iBottomLine, int iBottomCol, out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        public IntPtr GetWindowHandle()
        {
            return IntPtr.Zero;
        }

        public int GetWordExtent(int iLine, int iCol, uint dwFlags, TextSpan[] pSpan)
        {
            return VSConstants.S_OK;
        }

        public int HighlightMatchingBrace(uint dwFlags, uint cSpans, TextSpan[] rgBaseSpans)
        {
            return VSConstants.S_OK;
        }

        public int Initialize(IVsTextLines pBuffer, IntPtr hwndParent, uint InitFlags, INITVIEW[] pInitView)
        {
            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(int iLine, int cIndentLevels)
        {
            return VSConstants.S_OK;
        }

        public int RemoveCommandFilter(IOleCommandTarget pCmdTarg)
        {
            return VSConstants.S_OK;
        }

        public int ReplaceTextOnLine(int iLine, int iStartCol, int iCharsToReplace, string pszNewText, int iNewLen)
        {
            return VSConstants.S_OK;
        }

        public int RestrictViewRange(int iMinLine, int iMaxLine, IVsViewRangeClient pClient)
        {
            return VSConstants.S_OK;
        }

        public int SendExplicitFocus()
        {
            return VSConstants.S_OK;
        }

        public int SetBuffer(IVsTextLines pBuffer)
        {
            _buffer = pBuffer;
            return VSConstants.S_OK;
        }

        public int SetCaretPos(int iLine, int iColumn)
        {
            _caretLine = iLine;
            _caretColumn = iColumn; ;
            return VSConstants.S_OK;
        }

        public int SetScrollPosition(int iBar, int iFirstVisibleUnit)
        {
            return VSConstants.S_OK;
        }

        public int SetSelection(int iAnchorLine, int iAnchorCol, int iEndLine, int iEndCol)
        {
            _iAnchorLine = iAnchorLine;
            _iAnchorCol = iAnchorCol;
            _iEndLine = iEndLine;
            _iEndCol = iEndCol;
            return VSConstants.S_OK;
        }

        public int SetSelectionMode(TextSelMode iSelMode)
        {
            _selMode = iSelMode;
            return VSConstants.S_OK;
        }

        public int SetTopLine(int iBaseLine)
        {
            return VSConstants.S_OK;
        }

        public int UpdateCompletionStatus(IVsCompletionSet pCompSet, uint dwFlags)
        {
            return VSConstants.S_OK;
        }

        public int UpdateTipWindow(IVsTipWindow pTipWindow, uint dwFlags)
        {
            return VSConstants.S_OK;
        }

        public int UpdateViewFrameCaption()
        {
            return VSConstants.S_OK;
        }
    }
}
