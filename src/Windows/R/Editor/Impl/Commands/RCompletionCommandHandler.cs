// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Completions;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    /// <summary>
    /// Completion controller in R code editor
    /// </summary>
    internal sealed class RCompletionCommandHandler : CompletionCommandHandler {
        public RCompletionCommandHandler(ITextView textView) : base(textView) { }
        public override CompletionController CompletionController 
            => CompletionController.FromTextView<RCompletionController>(TextView);
    }
}
