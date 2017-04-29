// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public sealed class EditorLine : TextRange, IEditorLine {
        private ITextSnapshotLine _line;

        public EditorLine(ITextSnapshotLine line) : base(line.Start, line.Length) {
            _line = line;
        }

        public string LineBreak => _line.GetLineBreakText();
        public int LineNumber => _line.LineNumber;
        public string GetText() => _line.GetText();
        public override void Shift(int offset) { }
        public IEditorBufferSnapshot Snapshot => new EditorBufferSnapshot(_line.Snapshot);
    }
}
