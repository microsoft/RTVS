// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SmartIndent {
    /// <summary>
    /// Provides block and smart indentation in R code
    /// </summary>
    internal sealed class SmartIndent : ISmartIndent {
        private readonly ISmartIndenter _indenter;

        public static ISmartIndent FromView(ITextView textView, IREditorSettings settings)
            => textView.GetService<ISmartIndent>() ?? new SmartIndent(textView, settings);

        private SmartIndent(ITextView textView, IREditorSettings settings) {
            _indenter = SmartIndenter.FromView(textView.ToEditorView(), settings);
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line) => _indenter.GetDesiredIndentation(new EditorLine(line));
        public void Dispose() { }
    }
}
