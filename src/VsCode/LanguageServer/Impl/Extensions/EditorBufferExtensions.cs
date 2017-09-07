// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LanguageServer.VsCode.Contracts;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Extensions {
    internal static class EditorBufferExtensions {
        public static int ToStreamPosition(this IEditorBuffer editorBuffer, Position position)
            => editorBuffer.ToStreamPosition((int) position.Line, (int) position.Character);

        public static int ToStreamPosition(this IEditorBuffer editorBuffer, int lineNumber, int charNumber) {
            var line = editorBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            return line?.Start + charNumber ?? 0;
        }
    }
}
