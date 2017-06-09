// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

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
        public static void CommentBlock(IEditorView editorView, IEditorBuffer editorBuffer, ITextRange range, IEditorSupport es)
            => DoActionOnLines(editorView, editorBuffer, range, es, CommentLine, Resources.CommentSelection);

        /// <summary>
        /// Uncomments selected lines or current line if range has zero length.
        /// Only removes single comment. ### -> ## -> # and so on. Matches C# behavior.
        /// </summary>
        public static void UncommentBlock(IEditorView editorView, IEditorBuffer editorBuffer, ITextRange range, IEditorSupport es) 
            => DoActionOnLines(editorView, editorBuffer, range, es, UncommentLine, Resources.UncommentSelection);

        public static void DoActionOnLines(IEditorView editorView, IEditorBuffer editorBuffer, ITextRange range, IEditorSupport es, Func<IEditorLine, bool> action, string actionName) {
            // When user clicks editor margin to select a line, selection actually
            // ends in the beginning of the next line. In order to prevent commenting
            // of the next line that user did not select, we need to shrink span to
            // format and exclude the trailing line break.

            var snapshot = editorBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(range.End);

            int start = range.Start;
            int end = range.End;

            if (line.Start == range.End && range.Length > 0) {
                if (line.LineNumber > 0) {
                    line = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                    end = line.End;
                    start = Math.Min(start, end);
                }
            }

            int startLineNumber = editorBuffer.CurrentSnapshot.GetLineNumberFromPosition(start);
            int endLineNumber = editorBuffer.CurrentSnapshot.GetLineNumberFromPosition(end);

            using (var undoAction = es.CreateUndoAction(editorView)) {
                undoAction.Open(actionName);
                bool changed = false;
                for (int i = startLineNumber; i <= endLineNumber; i++) {
                    line = editorBuffer.CurrentSnapshot.GetLineFromLineNumber(i);
                    changed |= action(line);
                }
                if (changed) {
                    undoAction.Commit();
                }
            }
        }

        internal static bool CommentLine(IEditorLine line) {
            string lineText = line.GetText();
            if (!string.IsNullOrWhiteSpace(lineText)) {
                int leadingWsLength = lineText.Length - lineText.TrimStart().Length;
                line.Snapshot.EditorBuffer.Insert(line.Start + leadingWsLength, "#");
                return true;
            }

            return false;
        }

        internal static bool UncommentLine(IEditorLine line) {
            string lineText = line.GetText();
            if (!string.IsNullOrWhiteSpace(lineText)) {
                int leadingWsLength = lineText.Length - lineText.TrimStart().Length;
                if (leadingWsLength < lineText.Length) {
                    if (lineText[leadingWsLength] == '#') {
                        line.Snapshot.EditorBuffer.Delete(new TextRange(line.Start + leadingWsLength, 1));
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
