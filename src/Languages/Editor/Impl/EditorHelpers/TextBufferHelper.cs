using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.Languages.Editor.EditorHelpers {
    public class TextChangeExtent {
        public TextChangeExtent(int start, int oldEnd, int newEnd) {
            Start = start;
            OldEnd = oldEnd;
            NewEnd = newEnd;
        }

        public int Start { get; private set; }
        public int OldEnd { get; private set; }
        public int NewEnd { get; private set; }
    }

    public static class TextBufferExtensions {
        public static ITextView CurrentTextView(this ITextBuffer viewBuffer) {
            ITextView textView = null;
            if (viewBuffer != null) {
                TextViewData textViewData = TextViewConnectionListener.GetTextViewDataForBuffer(viewBuffer);
                if (textViewData != null) {
                    textView = textViewData.LastActiveView;
                }
            }

            return textView;
        }

        public static IEnumerable<ITextBuffer> GetContributingBuffers(this ITextBuffer textBuffer) {
            List<ITextBuffer> allBuffers = new List<ITextBuffer>();

            allBuffers.Add(textBuffer);
            for (int i = 0; i < allBuffers.Count; i++) {
                IProjectionBuffer currentBuffer = allBuffers[i] as IProjectionBuffer;
                if (currentBuffer != null) {
                    foreach (ITextBuffer sourceBuffer in currentBuffer.SourceBuffers) {
                        if (!allBuffers.Contains(sourceBuffer))
                            allBuffers.Add(sourceBuffer);
                    }
                }
            }

            return allBuffers;
        }

        public static bool IsContributingBuffer(this ITextBuffer buffer, ITextBuffer contributingBuffer) {
            return buffer.GetContributingBuffers().FirstOrDefault<ITextBuffer>((t) => t == contributingBuffer) != null;
        }

        public static string GetFileName(this ITextBuffer textBuffer) {
            string path = string.Empty;

            ITextDocument document = textBuffer.GetTextDocument();
            if (document != null && document.FilePath != null) {
                path = document.FilePath;
            }

            return path;
        }

        public static ITextDocument GetTextDocument(this ITextBuffer textBuffer) {
            ITextDocument document = null;

            IEnumerable<ITextBuffer> searchBuffers = textBuffer.GetContributingBuffers();

            foreach (ITextBuffer buffer in searchBuffers) {
                if (buffer.Properties.TryGetProperty(typeof(ITextDocument), out document)) {
                    break;
                }
            }

            return document;
        }

        public static bool IsFileName(this ITextBuffer textBuffer, string desiredFileName) {
            string filePath = textBuffer.GetFileName();
            string fileName = Path.GetFileName(filePath);

            return fileName == null ? false : String.Equals(fileName, desiredFileName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts stream buffer position to line and column.
        /// </summary>
        /// <returns>True if position was successfully converted</returns>
        public static bool GetLineColumnFromPosition(this ITextBuffer textBuffer, int position, out int line, out int column) {
            line = 0;
            column = 0;

            try {
                var textLine = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
                if (textLine != null) {
                    line = textLine.LineNumber;
                    column = position - textLine.Start.Position;

                    return true;
                }
            } catch (Exception) { }

            return false;
        }

        /// <summary>
        /// Converts line and column positions to a stream buffer position.
        /// </summary>
        /// <returns>Stream position or null if conversion failed</returns>
        public static int? GetPositionFromLineColumn(this ITextBuffer textBuffer, int line, int column) {
            return textBuffer.CurrentSnapshot.GetPositionFromLineColumn(line, column);
        }

        public static int? GetPositionFromLineColumn(this ITextSnapshot snapshot, int line, int column) {
            if ((line >= 0) && (line < snapshot.LineCount)) {
                ITextSnapshotLine textLine = snapshot.GetLineFromLineNumber(line);

                // Non-strict equality below, because caret can be position *after*
                // the last character of the line. So for line of length 1 both 
                // column 0 and column 1 are valid caret locations.
                if (column <= textLine.Length) {
                    return textLine.Start + column;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts text range positions to line and column coordinates.
        /// </summary>
        /// <returns>True if position was successfully converted</returns>
        public static bool GetLineColumnFromTextRange(this ITextBuffer textBuffer, ITextRange range,
                                                      out int startLine, out int startColumn,
                                                      out int endLine, out int endColumn) {
            startLine = endLine = 0;
            startColumn = endColumn = 0;

            if (GetLineColumnFromPosition(textBuffer, range.Start, out startLine, out startColumn)) {
                return GetLineColumnFromPosition(textBuffer, range.End, out endLine, out endColumn);
            }

            return false;
        }

        /// <summary>
        /// Converts line and column positions to a stream buffer range.
        /// </summary>
        /// <returns>Text range position or null if conversion failed</returns>
        public static ITextRange GetTextRangeFromLineColumn(this ITextBuffer textBuffer,
                                                            int startLine, int startColumn,
                                                            int endLine, int endColumn) {
            var start = GetPositionFromLineColumn(textBuffer, startLine, startColumn);
            if (start.HasValue) {
                var end = GetPositionFromLineColumn(textBuffer, endLine, endColumn);
                if (end.HasValue)
                    return TextRange.FromBounds(start.Value, end.Value);
            }

            return null;
        }

        public static bool ArePositionsOnTheSameLine(this ITextBuffer textBuffer, int position1, int position2) {
            int lineNumber1 = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position1);
            int lineNumber2 = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position2);
            return lineNumber1 == lineNumber2;
        }

        public static bool IsPositionOnLineNumber(this ITextBuffer textBuffer, int position, int lineNumber) {
            int positionLineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
            return positionLineNumber == lineNumber;
        }

        public static bool IsSignatureHelpBuffer(this ITextBuffer textBuffer) {
            return textBuffer.ContentType.TypeName.EndsWithIgnoreCase(" Signature Help");
        }

        public static string GetLineBreakText(this ITextBuffer textBuffer) {
            return (textBuffer.CurrentSnapshot.LineCount > 0)
                ? textBuffer.CurrentSnapshot.GetLineFromLineNumber(0).GetLineBreakText()
                : "\r\n";
        }

        public static void AddBufferDisposedAction(this ITextBuffer textBuffer, Action<ITextBuffer> callback) {
            if (EditorShell.Current != null) {
                ITextDocumentFactoryService textDocumentFactoryService = EditorShell.Current.ExportProvider.GetExport<ITextDocumentFactoryService>().Value;
                ITextDocument textDocument;

                if (textDocumentFactoryService.TryGetTextDocument(textBuffer, out textDocument)) {
                    EventHandler<TextDocumentEventArgs> onDocumentDisposed = null;
                    onDocumentDisposed = (object sender, TextDocumentEventArgs eventArgs) => {
                        if (eventArgs.TextDocument == textDocument) {
                            textDocumentFactoryService.TextDocumentDisposed -= onDocumentDisposed;

                            callback(textBuffer);
                        }
                    };

                    textDocumentFactoryService.TextDocumentDisposed += onDocumentDisposed;
                }
            }
        }

        // Returns spans corresponding to the changes that occurred between startVersion and endVersion
        public static bool GetChangedExtent(this ITextVersion oldVersion, ITextVersion newVersion, out Span? oldSpan, out Span? newSpan) {
            oldSpan = null;
            newSpan = null;

            if (oldVersion.VersionNumber > newVersion.VersionNumber) {
                // They've asked for information about an earlier snapshot, not supported
                Debug.Assert(false);
                return false;
            }

            int newEnd = Int32.MinValue;
            int position = Int32.MaxValue;
            int deltaLen = 0;
            while (oldVersion != newVersion) {
                INormalizedTextChangeCollection changes = oldVersion.Changes;
                if (changes.Count > 0) {
                    ITextChange firstChange = changes[0];
                    ITextChange lastChange = changes[changes.Count - 1];
                    int changeDeltaLen = lastChange.NewEnd - lastChange.OldEnd;

                    deltaLen += changeDeltaLen;

                    position = Math.Min(position, firstChange.NewPosition);

                    if (newEnd < lastChange.OldEnd) {
                        newEnd = lastChange.NewEnd;
                    } else {
                        newEnd += changeDeltaLen;
                    }
                }

                oldVersion = oldVersion.Next;
            }

            if (newEnd < position) {
                // There weren't any changes between the versions, return a null TextChangeExtent
                return false;
            }

            int oldEnd = newEnd - deltaLen;
            oldSpan = Span.FromBounds(position, oldEnd);
            newSpan = Span.FromBounds(position, newEnd);

            return true;
        }
    }
}
