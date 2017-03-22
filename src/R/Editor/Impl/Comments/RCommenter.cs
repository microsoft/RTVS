// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Comments {
    /// <summary>
    /// Provides functionality for comment/uncomment lines
    /// in R editor document
    /// </summary>
    public static class RCommenter {
        /// <summary>
        /// Comments selected lines or current line if range has zero length.
        /// Continues adding commentcharacter even if line is already commented.
        /// # -> ## -> ### and so on. Matches C# behavior.
        /// </summary>
        public static void CommentBlock(ITextView textView, ITextBuffer textBuffer, ITextRange range, IServiceContainer services) {
            DoActionOnLines(textView, textBuffer, range, services, CommentLine, Resources.CommentSelection);
        }

        /// <summary>
        /// Uncomments selected lines or current line if range has zero length.
        /// Only removes single comment. ### -> ## -> # and so on. Matches C# behavior.
        /// </summary>
        public static void UncommentBlock(ITextView textView, ITextBuffer textBuffer, ITextRange range, IServiceContainer services) {
            DoActionOnLines(textView, textBuffer, range, services, UncommentLine, Resources.UncommentSelection);
        }

        public static void DoActionOnLines(ITextView textView, ITextBuffer textBuffer, ITextRange range, IServiceContainer services, Func<ITextSnapshotLine, bool> action, string actionName) {
            // When user clicks editor margin to select a line, selection actually
            // ends in the beginning of the next line. In order to prevent commenting
            // of the next line that user did not select, we need to shrink span to
            // format and exclude the trailing line break.

            var snapshot = textBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(range.End);

            var start = range.Start;
            var end = range.End;

            if (line.Start.Position == range.End && range.Length > 0) {
                if (line.LineNumber > 0) {
                    line = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                    end = line.End.Position;
                    start = Math.Min(start, end);
                }
            }

            int startLineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(start);
            int endLineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(end);

            var es = services.GetService<IApplicationEditorSupport>();
            using (var undoAction = es.CreateCompoundAction(textView, textBuffer)) {
                undoAction.Open(actionName);
                var changed = false;
                for (int i = startLineNumber; i <= endLineNumber; i++) {
                    line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(i);
                    changed |= action(line);
                }
                if (changed) {
                    undoAction.Commit();
                }
            }
        }

        internal static bool CommentLine(ITextSnapshotLine line) {
            string lineText = line.GetText();
            if (!string.IsNullOrWhiteSpace(lineText)) {
                int leadingWsLength = lineText.Length - lineText.TrimStart().Length;
                line.Snapshot.TextBuffer.Insert(line.Start + leadingWsLength, "#");
                return true;
            }

            return false;
        }

        internal static bool UncommentLine(ITextSnapshotLine line) {
            var lineText = line.GetText();
            if (!string.IsNullOrWhiteSpace(lineText)) {
                int leadingWsLength = lineText.Length - lineText.TrimStart().Length;
                if (leadingWsLength < lineText.Length) {
                    if (lineText[leadingWsLength] == '#') {
                        line.Snapshot.TextBuffer.Delete(new Span(line.Start + leadingWsLength, 1));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
