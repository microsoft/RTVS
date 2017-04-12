// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.Languages.Editor.BraceMatch {
    public class BraceHighlightProvider : IViewTaggerProvider {
        private readonly ICoreShell _shell;

        public BraceHighlightProvider(ICoreShell shell) {
            _shell = shell;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer textBuffer) where T : ITag {
            BraceHighlighter highlighter = ServiceManager.GetService<BraceHighlighter>(textView);
            if (highlighter == null) {
                var document = textBuffer.GetService<IEditorDocument>();
                if (document != null) {
                    highlighter = new BraceHighlighter(textView, textBuffer, _shell);
                }
            }

            return highlighter as ITagger<T>;
        }
    }
}
