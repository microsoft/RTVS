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
            => context.Session.View.IsCaretInNamespace(out tripleColon);

        public static bool IsCaretInNamespace(this IEditorView view)
            => view.EditorBuffer.CurrentSnapshot.IsPositionInNamespace(view.Caret.Position.Position, out bool unused);

        public static bool IsCaretInNamespace(this IEditorView view, out bool tripleColon)
            => view.EditorBuffer.CurrentSnapshot.IsPositionInNamespace(view.Caret.Position.Position, out tripleColon);

        public static bool IsPositionInNamespace(this IEditorBufferSnapshot snapshot, int position, out bool tripleColon) {
            var doubleColon = position >= 2 && snapshot[position - 1] == ':' && snapshot[position - 2] == ':';
            tripleColon = position >= 3 && doubleColon && snapshot[position - 3] == ':';
            return doubleColon;
        }

        public static bool IsCaretInLibraryStatement(this IRIntellisenseContext context)
            => context.Session.View.IsCaretInLibraryStatement();

        public static bool IsCaretInLibraryStatement(this IEditorView view) {
            try {
                var caretPosition = view.Caret.Position.Position;
                var snapshot = view.EditorBuffer.CurrentSnapshot;
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
