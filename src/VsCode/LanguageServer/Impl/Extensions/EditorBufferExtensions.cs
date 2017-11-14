// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LanguageServer.VsCode.Contracts;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Extensions {
    internal static class EditorBufferExtensions {
        public static int ToStreamPosition(this IEditorBuffer editorBuffer, Position position)
            => editorBuffer.CurrentSnapshot.ToStreamPosition(position.Line, position.Character);

        public static int ToStreamPosition(this IEditorBuffer editorBuffer, int lineNumber, int charNumber)
            => editorBuffer.CurrentSnapshot.ToStreamPosition(lineNumber, charNumber);

        public static Position ToLinePosition(this IEditorBuffer editorBuffer, int position)
            => editorBuffer.CurrentSnapshot.ToLinePosition(position);

        public static Range ToLineRange(this IEditorBuffer editorBuffer, int start, int end)
            => new Range {
                Start = editorBuffer.CurrentSnapshot.ToLinePosition(start),
                End = editorBuffer.CurrentSnapshot.ToLinePosition(end)
            };

        public static Range ToLineRange(this ITextRange textRange, IEditorBuffer editorBuffer)
            => new Range {
                Start = editorBuffer.CurrentSnapshot.ToLinePosition(textRange.Start),
                End = editorBuffer.CurrentSnapshot.ToLinePosition(textRange.End)
            };

        public static Range ToLineRange(this IEditorBufferSnapshot snapshot, int start, int end)
            => new Range { Start = snapshot.ToLinePosition(start), End = snapshot.ToLinePosition(end) };

        public static Range ToLineRange(this ITextRange textRange, IEditorBufferSnapshot snapshot)
            => new Range { Start = snapshot.ToLinePosition(textRange.Start), End = snapshot.ToLinePosition(textRange.End) };

        public static Position ToLinePosition(this IEditorBufferSnapshot snapshot, int position) {
            var line = snapshot.GetLineFromPosition(position);
            return new Position { Line = line.LineNumber, Character = position - line.Start };
        }
        public static int ToStreamPosition(this Position position, IEditorBufferSnapshot snapshot)
            => snapshot.ToStreamPosition(position.Line, position.Character);

        public static int ToStreamPosition(this IEditorBufferSnapshot snapshot, int lineNumber, int charNumber) {
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            return line?.Start + charNumber ?? 0;
        }
        public static ITextRange ToTextRange(this Range range, IEditorBufferSnapshot snapshot) {
            var start = range.Start.ToStreamPosition(snapshot);
            var end = range.End.ToStreamPosition(snapshot);
            return TextRange.FromBounds(start, end);
        }
    }
}
