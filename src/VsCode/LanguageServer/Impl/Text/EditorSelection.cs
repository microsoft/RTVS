// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorSelection : TextRange, IEditorSelection {
        public EditorSelection(ITextRange range) : base(range) { }
        public SelectionMode Mode => SelectionMode.Stream;
        public ITextRange SelectedRange => this;
    }
}
