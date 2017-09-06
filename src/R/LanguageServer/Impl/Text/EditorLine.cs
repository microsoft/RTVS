// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorLine : TextRange, IEditorLine {
        public EditorLine(IEditorBufferSnapshot snapshot, ITextRange range, int lineNumber) : base(range) {
            Snapshot = snapshot;
            LineNumber = lineNumber;
        }
        public int LineNumber { get; }
        public string GetText() => Snapshot.GetText(this);

        public string LineBreak {
            get {
                if (Length > 0) {
                    if (Length >= 2 && Snapshot[End - 2].IsLineBreak()) {
                        return Snapshot.GetText(new TextRange(End - 2, 2));
                    }

                    if (Snapshot[End - 1].IsLineBreak()) {
                        return Snapshot.GetText(new TextRange(End - 1, 1));
                    }
                }
                return string.Empty;
            }
        }

        public IEditorBufferSnapshot Snapshot { get; }
    }
}
