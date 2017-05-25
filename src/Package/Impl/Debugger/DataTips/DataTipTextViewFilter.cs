// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.DataTips;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Debugger.DataTips {
    internal class DataTipTextViewFilter : IOleCommandTarget, IVsTextViewFilter, IDisposable {
        private readonly IVsDebugger _debugger;
        private readonly IWpfTextView _textView;
        private readonly IVsTextView _vsTextView;
        private readonly IVsTextLines _vsTextLines;
        private readonly IOleCommandTarget _nextTarget;

        private DataTipTextViewFilter(IWpfTextView textView, IVsEditorAdaptersFactoryService adapterService, IVsDebugger debugger) {
            Trace.Assert(textView.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType));

            _textView = textView;
            _debugger = debugger;

            _vsTextView = adapterService.GetViewAdapter(textView);
            _vsTextView.AddCommandFilter(this, out _nextTarget);
            _vsTextView.GetBuffer(out _vsTextLines);

            textView.Properties.AddProperty(typeof(DataTipTextViewFilter), this);
        }

        public void Dispose() {
            _textView.Properties.RemoveProperty(typeof(DataTipTextViewFilter));
            _vsTextView.RemoveCommandFilter(this);
        }

        public static DataTipTextViewFilter GetOrCreate(IWpfTextView textView, IVsEditorAdaptersFactoryService adapterService, IVsDebugger debugger)  {
            return textView.Properties.GetOrCreateSingletonProperty(() => new DataTipTextViewFilter(textView, adapterService, debugger));
        }

        public static DataTipTextViewFilter TryGet(IWpfTextView textView) {
            DataTipTextViewFilter result;
            textView.Properties.TryGetProperty(typeof(DataTipTextViewFilter), out result);
            return result;
        }

        public int GetDataTipText(TextSpan[] pSpan, out string pbstrText) {
            var doc = _textView.TextBuffer.GetEditorDocument<IREditorDocument>();
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

            var exprSpan = node.ToSnapshotSpan(doc.TextBuffer().CurrentSnapshot);
            SnapshotPointToLineAndColumnNumber(exprSpan.Start, out pSpan[0].iStartLine, out pSpan[0].iStartIndex);
            SnapshotPointToLineAndColumnNumber(exprSpan.End, out pSpan[0].iEndLine, out pSpan[0].iEndIndex);
            return _debugger.GetDataTipValue(_vsTextLines, pSpan, null, out pbstrText);
        }

        public int GetPairExtents(int iLine, int iIndex, TextSpan[] pSpan) => VSConstants.E_NOTIMPL;
        public int GetWordExtent(int iLine, int iIndex, uint dwFlags, TextSpan[] pSpan) => VSConstants.E_NOTIMPL;
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) 
            => _nextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) 
            => _nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

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
