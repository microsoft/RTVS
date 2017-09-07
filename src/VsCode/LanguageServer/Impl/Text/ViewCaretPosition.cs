// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class ViewCaretPosition : SnapshotPoint, IViewCaretPosition {
        public ViewCaretPosition(IEditorBufferSnapshot snapshot, int position)
            : base(snapshot, position) { }

        public int VirtualSpaces => 0;
    }
}
