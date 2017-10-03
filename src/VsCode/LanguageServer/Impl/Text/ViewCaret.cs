// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class ViewCaret: IViewCaret {
        private readonly IEditorView _view;
        private readonly int _position;

        public ViewCaret(IEditorView view, int position) {
            _view = view;
            _position = position;
            Position = new ViewCaretPosition(_view.EditorBuffer.CurrentSnapshot, _position);
        }

        public bool InVirtualSpace => false;
        public IViewCaretPosition Position { get; }

        public event EventHandler PositionChanged;
        public void MoveTo(int point, int virtualSpaces) { }
    }
}
