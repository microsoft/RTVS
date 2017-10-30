// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public static class RCompletionContextExtensions {
        public static bool IsCaretInNamespace(this IRIntellisenseContext context)
            => context.IsCaretInNamespace(out bool unused);

        public static bool IsCaretInNamespace(this IRIntellisenseContext context, out bool tripleColon)
            => context.Session.View.IsCaretInNamespace(context.EditorBuffer, out tripleColon);

        public static bool IsCaretInNamespace(this IEditorView view, IEditorBuffer editorBuffer)
            => view.IsCaretInNamespace(editorBuffer, out bool unused);

        public static bool IsCaretInNamespace(this IEditorView view, IEditorBuffer editorBuffer, out bool tripleColon) {
            tripleColon = false;
            // https://github.com/Microsoft/RTVS/issues/4187
            // Do not use view buffer since in REPL access may be slow b/c of the data
            // accumulated in the output part of the projection graph.
            var position = view.GetCaretPosition(editorBuffer);
            return position != null
                ? position.Snapshot.IsPositionInNamespace(position.Position, out tripleColon)
                : false;
        }

        public static bool IsPositionInNamespace(this IEditorBufferSnapshot snapshot, int position, out bool tripleColon) {
            var doubleColon = position >= 2 && snapshot[position - 1] == ':' && snapshot[position - 2] == ':';
            tripleColon = position >= 3 && doubleColon && snapshot[position - 3] == ':';
            return doubleColon;
        }

        public static bool IsCaretInLibraryStatement(this IRIntellisenseContext context)
            => context.Session.View.IsCaretInLibraryStatement(context.EditorBuffer);

        public static bool IsCaretInLibraryStatement(this IEditorView view, IEditorBuffer editorBuffer) {
            try {
                // https://github.com/Microsoft/RTVS/issues/4187
                // Do not use view buffer since in REPL access may be slow b/c of the data
                // accumulated in the output part of the projection graph.
                var position = view.GetCaretPosition(editorBuffer);
                if(position == null) {
                    return false;
                }

                var snapshot = editorBuffer.CurrentSnapshot;
                var caretPosition = position.Position;
                var line = snapshot.GetLineFromPosition(caretPosition);

                if (line.Length < 8 || caretPosition < line.Start + 8 || snapshot[caretPosition - 1] != '(') {
                    return false;
                }

                var start = -1;
                var end = -1;

                for (var i = caretPosition - 2; i >= 0; i--) {
                    if (!char.IsWhiteSpace(snapshot[i])) {
                        end = i + 1;
                        break;
                    }
                }

                if (end <= 0) {
                    return false;
                }

                for (var i = end - 1; i >= 0; i--) {
                    if (char.IsWhiteSpace(snapshot[i])) {
                        start = i + 1;
                        break;
                    } else if (i == 0) {
                        start = 0;
                        break;
                    }
                }

                if (start < 0 || end <= start) {
                    return false;
                }

                start -= line.Start;
                end -= line.Start;

                var s = line.GetText().Substring(start, end - start);
                if (s == "library" || s == "require") {
                    return true;
                }
            } catch (ArgumentException) { }

            return false;
        }
    }
}
