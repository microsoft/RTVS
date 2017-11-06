// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorLine : TextRange, IEditorLine {
        public int LineBreakLength { get; }

        public EditorLine(IEditorBufferSnapshot snapshot, int start, int length, int lineBreakLength, int lineNumber) :
            base(start, length) {
            LineBreakLength = lineBreakLength;
            Snapshot = snapshot;
            LineNumber = lineNumber;
        }
        public int LineNumber { get; }
        public string GetText() => Snapshot.GetText(this);
        public string LineBreak 
            => LineBreakLength > 0 ? Snapshot.GetText(new TextRange(End, LineBreakLength)) : string.Empty;

        public IEditorBufferSnapshot Snapshot { get; }
    }
}
