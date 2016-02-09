using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    /// <summary>
    /// Text view client that manages insertion of snippets
    /// </summary>
    public sealed class RExpansionClient : IVsExpansionClient {
        private static string[] ALL_STANDARD_SNIPPET_TYPES = { "Expansion", "SurroundsWith" };
        private static string[] SURROUND_WITH_SNIPPET_TYPES = { "SurroundsWith" };

        private IVsExpansionManager _expansionManager;
        private IVsExpansionSession _expansionSession;
        private IVsTextLines _viewTextLines;
        private IVsTextManager _textManager;

        internal ITextBuffer TextBuffer { get; private set; }

        private bool _earlyEndExpansionHappened = false;
        private string _shortcut = null;
        private string _title = null;

        public RExpansionClient(ITextView textView, ITextBuffer textBuffer) {
            TextBuffer = textBuffer;
            IVsTextBuffer bufferAdapter = textView.TextBuffer.QueryInterface<IVsTextBuffer>();

            _viewTextLines = bufferAdapter as IVsTextLines;
            _textManager = VsAppShell.Current.GetGlobalService<IVsTextManager>(typeof(SVsTextManager));
        }

        public static string[] GetAllStandardSnippetTypes() {
            return ALL_STANDARD_SNIPPET_TYPES;
        }

        public IVsExpansionManager ExpansionManager {
            get {
                if ((_expansionManager == null)) {
                    _expansionManager = CompletionUtilities.GetExpansionManager();
                }
                return _expansionManager;
            }
        }

        public SnippetListManager SnippetListManager {
            get {
                return SnippetListManager.FromContentType(TextBuffer.ContentType);
            }
        }

        public IVsTextView VsTextView {
            get {
                IVsTextView vsTextView = null;
                _textManager?.GetActiveView(0, _viewTextLines, out vsTextView);
                return vsTextView;
            }
        }

        public ITextView TextView {
            get {
                IVsEditorAdaptersFactoryService editorAdapterService = VsAppShell.Current.ExportProvider.GetExportedValue<IVsEditorAdaptersFactoryService>();
                return editorAdapterService.GetWpfTextView(this.VsTextView);
            }
        }

        public bool IsEditingExpansion() {
            return _expansionSession != null;
        }

        internal bool IsCaretInsideSnippetFields() {
            if (!IsEditingExpansion() || _viewTextLines == null)
                return false;

            // get the caret position
            int caretLine, caretCol;
            ErrorHandler.ThrowOnFailure(VsTextView.GetCaretPos(out caretLine, out caretCol));

            // Handle virtual space
            int lineLen;
            ErrorHandler.ThrowOnFailure(_viewTextLines.GetLengthOfLine(caretLine, out lineLen));
            if (caretCol > lineLen) {
                caretCol = lineLen;
            }

            // convert to stream position
            int caretPos;
            ErrorHandler.ThrowOnFailure(_viewTextLines.GetPositionOfLineIndex(caretLine, caretCol, out caretPos));

            // get the snippet span
            TextSpan[] pts = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_expansionSession.GetSnippetSpan(pts));
            TextSpan snippetSpan = pts[0];

            // convert to stream positions
            int snippetStart, snippetEnd;
            ErrorHandler.ThrowOnFailure(_viewTextLines.GetPositionOfLineIndex(snippetSpan.iStartLine, snippetSpan.iStartIndex, out snippetStart));
            ErrorHandler.ThrowOnFailure(_viewTextLines.GetPositionOfLineIndex(snippetSpan.iEndLine, snippetSpan.iEndIndex, out snippetEnd));

            IVsTextStream textStream = (IVsTextStream)_viewTextLines;

            // check to see if the caret position is inside one of the snippet fields
            IVsEnumStreamMarkers enumMarkers;
            if (VSConstants.S_OK == textStream.EnumMarkers(snippetStart, snippetEnd - snippetStart, 0, (uint)(ENUMMARKERFLAGS.EM_ALLTYPES | ENUMMARKERFLAGS.EM_INCLUDEINVISIBLE | ENUMMARKERFLAGS.EM_CONTAINED), out enumMarkers)) {
                IVsTextStreamMarker curMarker;
                while (VSConstants.S_OK == enumMarkers.Next(out curMarker)) {
                    int curMarkerPos;
                    int curMarkerLen;
                    if (VSConstants.S_OK == curMarker.GetCurrentSpan(out curMarkerPos, out curMarkerLen)) {
                        if (caretPos >= curMarkerPos && caretPos <= curMarkerPos + curMarkerLen) {
                            int markerType;
                            if (VSConstants.S_OK == curMarker.GetType(out markerType)) {
                                if (markerType == (int)MARKERTYPE2.MARKER_EXSTENCIL || markerType == (int)MARKERTYPE2.MARKER_EXSTENCIL_SELECTED)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public int InvokeInsertionUI(int invokationCommand) {
            if ((ExpansionManager != null) && (VsTextView != null)) {
                // Set the allowable snippet types and prompt text according to the current command.
                string[] snippetTypes = null;
                string promptText = "";
                if (invokationCommand == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET) {
                    snippetTypes = ALL_STANDARD_SNIPPET_TYPES;
                    promptText = Resources.InsertSnippet;
                } else if (invokationCommand == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH) {
                    snippetTypes = SURROUND_WITH_SNIPPET_TYPES;
                    promptText = Resources.SurroundWith;
                }

                return ExpansionManager.InvokeInsertionUI(
                    VsTextView,
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

        public int GoToNextExpansionField() {
            return _expansionSession.GoToNextExpansionField(0 /* fCommitIfLast - so don't commit */);
        }

        public int GoToPreviousExpansionField() {
            return _expansionSession.GoToPreviousExpansionField();
        }

        public int EndExpansionSession(bool leaveCaretWhereItIs) {
            return _expansionSession.EndCurrentExpansion(leaveCaretWhereItIs ? 1 : 0);
        }

        /// <summary>
        /// Inserts a snippet based on a shortcut string.
        /// </summary>
        public int StartSnippetInsertion(out bool snippetInserted) {
            int hr = VSConstants.E_FAIL;
            snippetInserted = false;
            bool removePrefixAngleBracket = false;

            // Get the text at the current caret position and
            // determine if it is a snippet shortcut.
            TextSpan ts = new TextSpan();
            ErrorHandler.ThrowOnFailure(VsTextView.GetCaretPos(out ts.iStartLine, out ts.iStartIndex));

            if (ts.iStartIndex > 0) {
                IVsTextLines textLines;
                ErrorHandler.ThrowOnFailure(VsTextView.GetBuffer(out textLines));

                // handle virtual space
                int lineLen;
                ErrorHandler.ThrowOnFailure(textLines.GetLengthOfLine(ts.iStartLine, out lineLen));
                if (ts.iStartIndex > lineLen) {
                    ts.iStartIndex = lineLen;
                }

                ts.iEndLine = ts.iStartLine;
                ts.iEndIndex = ts.iStartIndex;

                string line;
                ErrorHandler.ThrowOnFailure(textLines.GetLineText(ts.iStartLine, 0, ts.iStartLine, ts.iEndIndex, out line));

                // Find the start of the possible shortcut.
                if (ts.iStartIndex > line.Length) {
                    // The start point is past the end of the line, so don't try to search for a macro to expand. 
                    // This situation can happen when a new empty line is created by pressing the Enter key
                    // at the end of an existing line.
                } else {
                    while ((ts.iStartIndex > 0) && IsValidShortcutChar(line[ts.iStartIndex - 1])) {
                        ts.iStartIndex--;
                    }

                    // If we are inserting a snippet using Statement Completion list in markup, then 
                    // it is possible that we brought up tag name Statement Completion list using "<".
                    // Our snippet markup will always contain opening "<" in it's code. To avoid 
                    // having duplicate "<<", we will eat one of them up.
                }

                // If there appears to be a shortcut, insert the related snippet.
                if ((ts.iEndIndex - ts.iStartIndex) > 0) {
                    IVsExpansion expansion = textLines as IVsExpansion;
                    try {
                        TextSpan insertionPosition = ts;
                        if (removePrefixAngleBracket) {
                            insertionPosition.iStartIndex--;
                        }

                        _earlyEndExpansionHappened = false;
                        _shortcut = line.Substring(ts.iStartIndex, ts.iEndIndex - ts.iStartIndex);

                        // check to make sure the shortcut is available before committing it

                        var combinedSnippetInfos = SnippetListManager.GetSnippetList(TextBuffer);
                        SnippetInfo shortcutSnippetInfo = null;

                        if (combinedSnippetInfos != null) {
                            foreach (var snippetInfo in combinedSnippetInfos) {
                                if ((_shortcut != null) && (string.Compare(snippetInfo.Shortcut, _shortcut, StringComparison.Ordinal) == 0)) {
                                    shortcutSnippetInfo = snippetInfo;
                                    break;
                                }
                            }
                        }

                        if (shortcutSnippetInfo != null) {
                            string snippetFilePath = shortcutSnippetInfo.Path;
                            string snippetTitle = shortcutSnippetInfo.Title;
                            hr = expansion.InsertNamedExpansion(snippetTitle, snippetFilePath, insertionPosition, this, RGuidList.RLanguageServiceGuid, 0, out _expansionSession);

                            if (_earlyEndExpansionHappened) {
                                // EndExpansion was called before InsertExpansion returned, so set _expansionSession
                                // to null to indicate that there is no active expansion session. This can occur when 
                                // the snippet inserted doesn't have any expansion fields.
                                _expansionSession = null;
                                _earlyEndExpansionHappened = false;
                                _shortcut = null;
                                _title = null;
                            }

                            ErrorHandler.ThrowOnFailure(hr);
                            snippetInserted = true;
                            return hr;
                        }
                    } catch {
                        // The text before the cursor probably wasn't a valid snippet shortcut, so allow the next filter 
                        // to handle the command.
                    }
                }
            }

            return hr;
        }

        public static bool IsValidShortcutChar(char character) {
            // TODO: {alexgav} Make this settable via the registry and per-language
            return char.IsLetterOrDigit(character) ||
                   (character == ':') ||
                   (character == '/') ||
                   (character == '-') ||
                   (character == '@');
        }

        #region IVsExpansionClient Members

        int IVsExpansionClient.EndExpansion() {
            if (_expansionSession == null) {
                _earlyEndExpansionHappened = true;
            } else {
                _expansionSession = null;
            }
            _title = null;
            _shortcut = null;

            return VSConstants.S_OK;
        }

        int IVsExpansionClient.FormatSpan(IVsTextLines pBuffer, TextSpan[] ts) {
            int hr = VSConstants.S_OK;
            if (_viewTextLines != null) {
                int startPos = -1;
                int endPos = -1;
                if (ErrorHandler.Succeeded(_viewTextLines.GetPositionOfLineIndex(ts[0].iStartLine, ts[0].iStartIndex, out startPos)) &&
                    ErrorHandler.Succeeded(_viewTextLines.GetPositionOfLineIndex(ts[0].iEndLine, ts[0].iEndIndex, out endPos))) {
                    SnapshotSpan viewSpan = new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, startPos, endPos - startPos);

                    NormalizedSnapshotSpanCollection mappedSpans = TextView.BufferGraph.MapDownToBuffer(
                        viewSpan, SpanTrackingMode.EdgeInclusive, TextBuffer);
                    Debug.Assert(mappedSpans.Count == 1);

                    if (mappedSpans.Count > 0) {
                        IREditorDocument document = REditorDocument.TryFromTextBuffer(TextBuffer);
                        if (document != null) {
                            document.EditorTree.EnsureTreeReady();

                            RangeFormatter.FormatRange(TextView, TextBuffer, 
                                new TextRange(mappedSpans[0].Start, mappedSpans[0].Length), 
                                document.EditorTree.AstRoot, REditorSettings.FormatOptions);
                        }
                    }
                }
            }
            return hr;
        }

        public int GetExpansionFunction(MSXML.IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc) {
            pFunc = null;
            return VSConstants.S_OK;
        }

        int IVsExpansionClient.IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind) {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        int IVsExpansionClient.IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType) {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        int IVsExpansionClient.OnAfterInsertion(IVsExpansionSession pSession) {
            return VSConstants.S_OK;
        }

        int IVsExpansionClient.OnBeforeInsertion(IVsExpansionSession pSession) {
            return VSConstants.S_OK;
        }

        int IVsExpansionClient.OnItemChosen(string pszTitle, string pszPath) {
            // A snippet was chosen, so insert it at the current caret location.
            int hr = VSConstants.E_FAIL;
            try {
                TextSpan ts;
                IVsTextView textView = VsTextView;
                ErrorHandler.ThrowOnFailure(textView.GetCaretPos(out ts.iStartLine, out ts.iStartIndex));

                IVsTextLines textLines;
                ErrorHandler.ThrowOnFailure(textView.GetBuffer(out textLines));

                // Dev10 778675: Handle virtual space
                int lineLen;
                ErrorHandler.ThrowOnFailure(textLines.GetLengthOfLine(ts.iStartLine, out lineLen));

                if (ts.iStartIndex > lineLen) {
                    ts.iStartIndex = lineLen;
                }

                ts.iEndLine = ts.iStartLine;
                ts.iEndIndex = ts.iStartIndex;

                string line;
                ErrorHandler.ThrowOnFailure(textLines.GetLineText(ts.iStartLine, 0, ts.iStartLine, ts.iEndIndex, out line));

                IVsExpansion expansion = textLines as IVsExpansion;
                _earlyEndExpansionHappened = false;
                _title = pszTitle;

                hr = expansion.InsertNamedExpansion(pszTitle, pszPath, ts, this, RGuidList.RLanguageServiceGuid, 0, out _expansionSession);
                if (_earlyEndExpansionHappened) {
                    // EndExpansion was called before InsertNamedExpansion returned, so set _expansionSession
                    // to null to indicate that there is no active expansion session. This can occur when 
                    // the snippet inserted doesn't have any expansion fields.
                    _expansionSession = null;
                    _earlyEndExpansionHappened = false;
                    _title = null;
                    _shortcut = null;
                }
                ErrorHandler.ThrowOnFailure(hr);
            } catch (COMException ex) {
                hr = ex.ErrorCode;
            } catch (Exception) { }
            return hr;
        }

        int IVsExpansionClient.PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts) {
            return VSConstants.S_OK;
        }
        #endregion
    }
}
