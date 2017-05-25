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
        public static bool IsCaretInNameSpace(this IRIntellisenseContext context)
            => context.Session.View.IsCaretInNamespace(context.EditorBuffer);

        public static bool IsCaretInNamespace(this IEditorView view, IEditorBuffer editorBuffer = null) {
            editorBuffer = editorBuffer ?? view.EditorBuffer;
            var bufferPosition = view.GetCaretPosition(editorBuffer);
            if (bufferPosition != null) {
                return bufferPosition.Snapshot.IsPositionInNamespace(bufferPosition.Position);
            }
            return false;
        }

        public static bool IsCaretInNamespace(this IRIntellisenseContext context, IEditorView view) {
            var bufferPosition = context.Session.View.GetCaretPosition(context.EditorBuffer);
            if (bufferPosition != null) {
                return bufferPosition.Snapshot.IsPositionInNamespace(bufferPosition.Position);
            }
            return false;
        }

        public static bool IsPositionInNamespace(this IEditorBufferSnapshot snapshot, int position) {
            if (position > 0) {
                var line = snapshot.GetLineFromPosition(position);
                if (line.Length > 2 && position - line.Start > 2) {
                    return snapshot[position - 1] == ':';
                }
            }
            return false;
        }

        public static bool IsCaretInLibraryStatement(this IRIntellisenseContext context)
            => context.Session.View.IsCaretInLibraryStatement(context.EditorBuffer);

        public static bool IsCaretInLibraryStatement(this IEditorView view, IEditorBuffer editorBuffer = null) {
            try {
                editorBuffer = editorBuffer ?? view.EditorBuffer;
                var bufferPosition = view.GetCaretPosition(editorBuffer);
                if (bufferPosition != null) {
                    var snapshot = bufferPosition.Snapshot;
                    int caretPosition = bufferPosition.Position;
                    var line = snapshot.GetLineFromPosition(caretPosition);

                    if (line.Length < 8 || caretPosition < line.Start + 8 || snapshot[caretPosition - 1] != '(') {
                        return false;
                    }

                    int start = -1;
                    int end = -1;

                    for (int i = caretPosition - 2; i >= 0; i--) {
                        if (!char.IsWhiteSpace(snapshot[i])) {
                            end = i + 1;
                            break;
                        }
                    }

                    if (end <= 0) {
                        return false;
                    }

                    for (int i = end - 1; i >= 0; i--) {
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

                    string s = line.GetText().Substring(start, end - start);
                    if (s == "library" || s == "require") {
                        return true;
                    }
                }
            } catch (ArgumentException) { }

            return false;
        }
    }
}
