// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class ViewCaret: IViewCaret {
        private readonly IEditorView _view;

        public ViewCaret(IEditorView view) {
            _view = view;
        }

        public bool InVirtualSpace => false;
        public IViewCaretPosition Position => new ViewCaretPosition(_view.EditorBuffer.CurrentSnapshot, 0);

        public event EventHandler PositionChanged;
        public void MoveTo(int point, int virtualSpaces) { }
    }
}
