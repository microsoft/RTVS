// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    /// <summary>
    /// Text view client that manages insertion of snippets
    /// </summary>
    public sealed class ExpansionClient : IVsExpansionClient {
        private static readonly string[] AllStandardSnippetTypes = { "Expansion", "SurroundsWith" };
        private static readonly string[] SurroundWithSnippetTypes = { "SurroundsWith" };
        private const string _replContentTypeName = "Interactive Content";

        private IVsExpansionManager _expansionManager;
        private IVsExpansionSession _expansionSession;
        private IExpansionsCache _cache;

        private bool _earlyEndExpansionHappened = false;

        public ExpansionClient(ITextView textView, ITextBuffer textBuffer, IVsExpansionManager expansionManager, IExpansionsCache cache) {
            TextView = textView;
            TextBuffer = textBuffer;
            _expansionManager = expansionManager;
            _cache = cache;
        }

        public ITextBuffer TextBuffer { get; }
        public ITextView TextView { get; }

        internal IVsExpansionSession Session => _expansionSession;

        public bool IsEditingExpansion() {
            return _expansionSession != null;
        }

        internal bool IsCaretInsideSnippetFields() {
            if (!IsEditingExpansion() || TextView.Caret.InVirtualSpace) {
                return false;
            }

            // Get the snippet span
            TextSpan[] pts = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(Session.GetSnippetSpan(pts));
            TextSpan snippetSpan = pts[0];

            // Convert text span to stream positions
            int snippetStart, snippetEnd;
            var vsTextLines = GetTargetBuffer().GetBufferAdapter<IVsTextLines>();
            ErrorHandler.ThrowOnFailure(vsTextLines.GetPositionOfLineIndex(snippetSpan.iStartLine, snippetSpan.iStartIndex, out snippetStart));
            ErrorHandler.ThrowOnFailure(vsTextLines.GetPositionOfLineIndex(snippetSpan.iEndLine, snippetSpan.iEndIndex, out snippetEnd));

            var textStream = (IVsTextStream)vsTextLines;

            // check to see if the caret position is inside one of the snippet fields
            IVsEnumStreamMarkers enumMarkers;
            if (VSConstants.S_OK == textStream.EnumMarkers(snippetStart, snippetEnd - snippetStart, 0, (uint)(ENUMMARKERFLAGS.EM_ALLTYPES | ENUMMARKERFLAGS.EM_INCLUDEINVISIBLE | ENUMMARKERFLAGS.EM_CONTAINED), out enumMarkers)) {
                IVsTextStreamMarker curMarker;
                var span = SpanFromViewSpan(new Span(TextView.Caret.Position.BufferPosition, 0));
                if(span.HasValue) { 
                while (VSConstants.S_OK == enumMarkers.Next(out curMarker)) {
                    int curMarkerPos;
                    int curMarkerLen;
                        if (VSConstants.S_OK == curMarker.GetCurrentSpan(out curMarkerPos, out curMarkerLen)) {
                            if (span.Value.Start >= curMarkerPos && span.Value.Start <= curMarkerPos + curMarkerLen) {
                                int markerType;
                                if (VSConstants.S_OK == curMarker.GetType(out markerType)) {
                                    if (markerType == (int)MARKERTYPE2.MARKER_EXSTENCIL || markerType == (int)MARKERTYPE2.MARKER_EXSTENCIL_SELECTED) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public int InvokeInsertionUI(int invokationCommand) {
            if ((_expansionManager != null) && (TextView != null)) {
                // Set the allowable snippet types and prompt text according to the current command.
                string[] snippetTypes = null;
                string promptText = "";
                if (invokationCommand == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET) {
                    snippetTypes = AllStandardSnippetTypes;
                    promptText = Resources.InsertSnippet;
                } else if (invokationCommand == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH) {
                    snippetTypes = SurroundWithSnippetTypes;
                    promptText = Resources.SurrondWithSnippet;
                }

                return _expansionManager.InvokeInsertionUI(
                    TextView.GetViewAdapter<IVsTextView>(),
                    this,
                    RGuidList.RLanguageServiceGuid,
                    snippetTypes,
                    (snippetTypes != null) ? snippetTypes.Length : 0,
                    0,
                    null, // Snippet kinds
                    0,    // Length of snippet kinds
                    0,
                    promptText,
                    "\t");
            }

            return VSConstants.E_UNEXPECTED;
        }

        public int GoToNextExpansionField() => Session.GoToNextExpansionField(0 /* fCommitIfLast - so don't commit */);
        public int GoToPreviousExpansionField() => Session.GoToPreviousExpansionField();
        public int EndExpansionSession(bool leaveCaretWhereItIs) => Session.EndCurrentExpansion(leaveCaretWhereItIs ? 1 : 0);

        /// <summary>
        /// Inserts a snippet based on a shortcut string.
        /// </summary>
        public int StartSnippetInsertion(out bool snippetInserted) {
            int hr = VSConstants.E_FAIL;
            snippetInserted = false;

            // Get the text at the current caret position and
            // determine if it is a snippet shortcut.
            if (!TextView.Caret.InVirtualSpace) {
                var expansion = GetTargetBuffer().GetBufferAdapter<IVsExpansion>();
                _earlyEndExpansionHappened = false;

                Span span;
                var shortcut = TextView.GetItemBeforeCaret(out span, x => true);
                VsExpansion? exp = _cache.GetExpansion(shortcut);

                var ts = TextSpanFromViewSpan(span);
                if (exp.HasValue && ts.HasValue) {
                    // Insert into R buffer
                    hr = expansion.InsertNamedExpansion(exp.Value.title, exp.Value.path, ts.Value, this, RGuidList.RLanguageServiceGuid, 0, out _expansionSession);
                    if (_earlyEndExpansionHappened) {
                        // EndExpansion was called before InsertExpansion returned, so set _expansionSession
                        // to null to indicate that there is no active expansion session. This can occur when 
                        // the snippet inserted doesn't have any expansion fields.
                        _expansionSession = null;
                        _earlyEndExpansionHappened = false;
                    }
                    ErrorHandler.ThrowOnFailure(hr);
                    snippetInserted = true;
                    return hr;
                }
            }
            return hr;
        }

        #region IVsExpansionClient
        public int EndExpansion() {
            if (_expansionSession == null) {
                _earlyEndExpansionHappened = true;
            } else {
                _expansionSession = null;
            }
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines vsTextLines, TextSpan[] ts) {
            int hr = VSConstants.S_OK;
            int startPos = -1;
            int endPos = -1;
            if (ErrorHandler.Succeeded(vsTextLines.GetPositionOfLineIndex(ts[0].iStartLine, ts[0].iStartIndex, out startPos)) &&
                ErrorHandler.Succeeded(vsTextLines.GetPositionOfLineIndex(ts[0].iEndLine, ts[0].iEndIndex, out endPos))) {
                var textBuffer = vsTextLines.ToITextBuffer();
                RangeFormatter.FormatRange(TextView, textBuffer, TextRange.FromBounds(startPos, endPos), REditorSettings.FormatOptions);
            }
            return hr;
        }

        public int GetExpansionFunction(MSXML.IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc) {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind) {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType) {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession) {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession) {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(string pszTitle, string pszPath) {
            int hr = VSConstants.E_FAIL;
            if (!TextView.Caret.InVirtualSpace) {
                var span = new Span(TextView.Caret.Position.BufferPosition, 0);
                var ts = TextSpanFromViewSpan(span);
                if (ts.HasValue) {
                    var expansion = GetTargetBuffer().GetBufferAdapter<IVsExpansion>();
                    _earlyEndExpansionHappened = false;

                    hr = expansion.InsertNamedExpansion(pszTitle, pszPath, ts.Value, this, RGuidList.RLanguageServiceGuid, 0, out _expansionSession);
                    if (_earlyEndExpansionHappened) {
                        // EndExpansion was called before InsertNamedExpansion returned, so set _expansionSession
                        // to null to indicate that there is no active expansion session. This can occur when 
                        // the snippet inserted doesn't have any expansion fields.
                        _expansionSession = null;
                        _earlyEndExpansionHappened = false;
                    }
                }
            }
            return hr;
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts) {
            return VSConstants.S_OK;
        }
        #endregion

        private static TextSpan TextSpanFromSpan(ITextBuffer textBuffer, Span span) {
            var ts = new TextSpan();
            ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromPosition(span.Start);
            ts.iStartLine = line.LineNumber;
            ts.iEndLine = line.LineNumber;
            ts.iStartIndex = span.Start - line.Start;
            ts.iEndIndex = span.End - line.Start;
            return ts;
        }

        private ITextBuffer GetTargetBuffer() {
            if (IsRepl) {
                var document = REditorDocument.FindInProjectedBuffers(TextView.TextBuffer);
                return document?.TextBuffer;
            }
            return TextView.TextBuffer;
        }

        private Span? SpanFromViewSpan(Span span) {
            var textBuffer = GetTargetBuffer();
            if (IsRepl) {
                // Map it down to R buffer
                var start = TextView.MapDownToR(span.Start);
                var end = TextView.MapDownToR(span.End);
                if (!start.HasValue || !end.HasValue) {
                    return null;
                }
                return Span.FromBounds(start.Value, end.Value);
            }
            return span;
        }

        private TextSpan? TextSpanFromViewSpan(Span span) {
            var textBuffer = GetTargetBuffer();
            if (IsRepl) {
                // Map it down to R buffer
                var start = TextView.MapDownToR(span.Start);
                var end = TextView.MapDownToR(span.End);
                if (!start.HasValue || !end.HasValue) {
                    return null;
                }
                span = Span.FromBounds(start.Value, end.Value);
            }
            return TextSpanFromSpan(textBuffer, span);
        }

        private bool IsRepl => TextView.TextBuffer.ContentType.TypeName.EqualsIgnoreCase(_replContentTypeName);
    }
}
