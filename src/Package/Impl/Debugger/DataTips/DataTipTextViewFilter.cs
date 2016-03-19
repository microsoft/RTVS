// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.DataTips;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Debugger.DataTips {
    internal class DataTipTextViewFilter : IOleCommandTarget, IVsTextViewFilter {
        private readonly IVsEditorAdaptersFactoryService _adapterService;
        private readonly IVsDebugger _debugger;
        private readonly IWpfTextView _textView;
        private readonly IVsTextLines _vsTextLines;
        private readonly IOleCommandTarget _nextTarget;

        public DataTipTextViewFilter(IWpfTextView textView, IVsDebugger debugger) {
            if (!textView.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                return;
            }

            _textView = textView;
            _debugger = debugger;
            _adapterService = ComponentLocator<IVsEditorAdaptersFactoryService>.Import();

            var vsTextView = _adapterService.GetViewAdapter(textView);
            vsTextView.AddCommandFilter(this, out _nextTarget);

            vsTextView.GetBuffer(out _vsTextLines);
        }

        public int GetDataTipText(TextSpan[] pSpan, out string pbstrText) {
            var doc = REditorDocument.FromTextBuffer(_textView.TextBuffer);
            var ast = doc?.EditorTree?.AstRoot;
            if (ast == null) {
                pbstrText = null;
                return VSConstants.E_FAIL;
            }

            var snapshot = _textView.TextSnapshot;
            var start = LineAndColumnNumberToSnapshotPoint(snapshot, pSpan[0].iStartLine, pSpan[0].iStartIndex);
            var end = LineAndColumnNumberToSnapshotPoint(snapshot, pSpan[0].iEndLine, pSpan[0].iEndIndex);
            var range = new SnapshotSpan(start, end).ToTextRange();
            var node = RDataTip.GetDataTipExpression(ast, range);
            if (node == null) {
                pbstrText = null;
                return VSConstants.E_FAIL;
            }

            var exprSpan = node.ToSnapshotSpan(doc.TextBuffer.CurrentSnapshot);
            SnapshotPointToLineAndColumnNumber(exprSpan.Start, out pSpan[0].iStartLine, out pSpan[0].iStartIndex);
            SnapshotPointToLineAndColumnNumber(exprSpan.End, out pSpan[0].iEndLine, out pSpan[0].iEndIndex);
            return _debugger.GetDataTipValue(_vsTextLines, pSpan, null, out pbstrText);
        }

        public int GetPairExtents(int iLine, int iIndex, TextSpan[] pSpan) {
            return VSConstants.E_NOTIMPL;
        }

        public int GetWordExtent(int iLine, int iIndex, uint dwFlags, TextSpan[] pSpan) {
            return VSConstants.E_NOTIMPL;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return _nextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return _nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private static SnapshotPoint LineAndColumnNumberToSnapshotPoint(ITextSnapshot snapshot, int lineNumber, int columnNumber) {
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            var snapshotPoint = new SnapshotPoint(snapshot, line.Start + columnNumber);
            return snapshotPoint;
        }

        private static void SnapshotPointToLineAndColumnNumber(SnapshotPoint snapshotPoint, out int lineNumber, out int columnNumber) {
            var line = snapshotPoint.GetContainingLine();
            lineNumber = line.LineNumber;
            columnNumber = snapshotPoint.Position - line.Start.Position;
        }
    }
}
